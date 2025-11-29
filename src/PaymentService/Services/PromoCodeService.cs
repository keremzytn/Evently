using System.Collections.Concurrent;
using PaymentService.DTOs;
using PaymentService.Models;
using Shared.Events;
using Shared.Kafka;

namespace PaymentService.Services;

public class PromoCodeService : IPromoCodeService
{
    private readonly ConcurrentDictionary<string, PromoCode> _catalog;
    private readonly List<PromoUsage> _usages = new();
    private readonly List<PromoValidationLog> _logs = new();
    private readonly object _syncRoot = new();
    private readonly ILogger<PromoCodeService> _logger;
    private readonly KafkaProducer? _kafkaProducer;

    public PromoCodeService(IConfiguration configuration, ILogger<PromoCodeService> logger, KafkaProducer? kafkaProducer)
    {
        _logger = logger;
        _kafkaProducer = kafkaProducer;
        var seeded = configuration.GetSection("PromoCodes").Get<List<PromoCode>>() ?? new List<PromoCode>();
        foreach (var promo in seeded)
        {
            promo.Code = promo.Code.ToUpperInvariant();
            promo.ValidFrom ??= DateTime.UtcNow.AddDays(-1);
            promo.ValidUntil ??= DateTime.UtcNow.AddMonths(1);
        }

        _catalog = new ConcurrentDictionary<string, PromoCode>(seeded.ToDictionary(p => p.Code), StringComparer.OrdinalIgnoreCase);
    }

    public Task<PromoValidationResponseDto> ValidateAsync(PromoValidationRequestDto request, string clientIp)
    {
        var response = ValidateInternal(request, clientIp, false, null, null, out _, out _);
        return Task.FromResult(response);
    }

    public async Task<PromoValidationResponseDto> RedeemAsync(PromoRedeemRequestDto request, string clientIp)
    {
        var response = ValidateInternal(request, clientIp, true, request.OrderId, request.Channel, out var discount, out var promo);
        if (response.IsValid && promo != null)
        {
            await PublishPromoUsageAsync(request, discount, promo);
        }

        return response;
    }

    public Task<IReadOnlyCollection<PromoCode>> GetCatalogAsync()
    {
        return Task.FromResult<IReadOnlyCollection<PromoCode>>(_catalog.Values.ToList());
    }

    public Task<IReadOnlyCollection<PromoValidationLog>> GetLogsAsync(int limit = 100)
    {
        lock (_syncRoot)
        {
            var ordered = _logs.OrderByDescending(l => l.Timestamp).Take(limit).ToList();
            return Task.FromResult<IReadOnlyCollection<PromoValidationLog>>(ordered);
        }
    }

    private PromoValidationResponseDto ValidateInternal(
        PromoValidationRequestDto request,
        string clientIp,
        bool commitUsage,
        string? orderId,
        string? channel,
        out decimal discountAmount,
        out PromoCode? promoUsed)
    {
        var normalizedCode = request.Code.Trim().ToUpperInvariant();
        PromoValidationResponseDto response;
        PromoCode? promo;
        discountAmount = 0;
        promoUsed = null;
        lock (_syncRoot)
        {
            if (!_catalog.TryGetValue(normalizedCode, out promo))
            {
                response = BuildResponse(false, "NOT_FOUND", 0, null, null);
                AppendLog(normalizedCode, request, clientIp, response);
                return response;
            }

            promoUsed = promo;

            var now = DateTime.UtcNow;
            if ((promo.ValidFrom.HasValue && now < promo.ValidFrom.Value) || (promo.ValidUntil.HasValue && now > promo.ValidUntil.Value))
            {
                response = BuildResponse(false, "OUT_OF_RANGE", 0, promo, promo.Currency);
                AppendLog(normalizedCode, request, clientIp, response);
                return response;
            }

            if (promo.Status != PromoStatus.Active)
            {
                response = BuildResponse(false, "INACTIVE", 0, promo, promo.Currency);
                AppendLog(normalizedCode, request, clientIp, response);
                return response;
            }

            if (promo.MinOrderAmount.HasValue && request.CartTotal < promo.MinOrderAmount.Value)
            {
                response = BuildResponse(false, "MIN_AMOUNT", 0, promo, promo.Currency);
                AppendLog(normalizedCode, request, clientIp, response);
                return response;
            }

            if (promo.AppliesTo == PromoScope.Event && !string.Equals(promo.TargetId, request.EventId, StringComparison.OrdinalIgnoreCase))
            {
                response = BuildResponse(false, "WRONG_EVENT", 0, promo, promo.Currency);
                AppendLog(normalizedCode, request, clientIp, response);
                return response;
            }

            if (promo.AppliesTo == PromoScope.Organizer && !string.Equals(promo.TargetId, request.OrganizerId, StringComparison.OrdinalIgnoreCase))
            {
                response = BuildResponse(false, "WRONG_ORG", 0, promo, promo.Currency);
                AppendLog(normalizedCode, request, clientIp, response);
                return response;
            }

            var totalUsage = _usages.Count(u => u.PromoCode == normalizedCode);
            if (promo.UsageLimit.HasValue && totalUsage >= promo.UsageLimit.Value)
            {
                response = BuildResponse(false, "USAGE_LIMIT", 0, promo, promo.Currency);
                AppendLog(normalizedCode, request, clientIp, response);
                return response;
            }

            var userUsage = _usages.Count(u => u.PromoCode == normalizedCode && u.UserId == request.UserId);
            if (promo.PerUserLimit.HasValue && userUsage >= promo.PerUserLimit.Value)
            {
                response = BuildResponse(false, "USER_LIMIT", 0, promo, promo.Currency);
                AppendLog(normalizedCode, request, clientIp, response);
                return response;
            }

            var discount = CalculateDiscount(promo, request.CartTotal);
            response = BuildResponse(true, "OK", discount, promo, promo.Currency);
            discountAmount = discount;

            if (commitUsage)
            {
                var usage = new PromoUsage
                {
                    PromoCode = normalizedCode,
                    UserId = request.UserId,
                    OrderId = string.IsNullOrWhiteSpace(orderId) ? Guid.NewGuid().ToString("N") : orderId,
                    DiscountAmount = discount,
                    Channel = channel ?? "web",
                    UsedAt = DateTime.UtcNow
                };

                _usages.Add(usage);
            }

            AppendLog(normalizedCode, request, clientIp, response);
            TrimLogBuffer();
        }

        return response;
    }

    private void AppendLog(string code, PromoValidationRequestDto request, string clientIp, PromoValidationResponseDto response)
    {
        _logs.Add(new PromoValidationLog
        {
            PromoCode = code,
            UserId = request.UserId,
            EventId = request.EventId ?? string.Empty,
            IsValid = response.IsValid,
            Reason = response.ReasonCode,
            CartTotal = request.CartTotal,
            Timestamp = DateTime.UtcNow,
            ClientIp = clientIp
        });
    }

    private void TrimLogBuffer()
    {
        const int maxLogs = 1000;
        if (_logs.Count <= maxLogs)
        {
            return;
        }

        var removeCount = _logs.Count - maxLogs;
        _logs.RemoveRange(0, removeCount);
    }

    private static PromoValidationResponseDto BuildResponse(bool isValid, string reason, decimal discount, PromoCode? promo, string? currency)
    {
        return new PromoValidationResponseDto
        {
            IsValid = isValid,
            ReasonCode = reason,
            DiscountAmount = Math.Round(discount, 2, MidpointRounding.AwayFromZero),
            Currency = currency ?? "TRY",
            PromoId = promo?.Id,
            Type = promo?.Type
        };
    }

    private static decimal CalculateDiscount(PromoCode promo, decimal cartTotal)
    {
        decimal discount = promo.Type switch
        {
            PromoType.Percentage => cartTotal * promo.Value / 100m,
            PromoType.Fixed => promo.Value,
            PromoType.Bogo => Math.Min(cartTotal / 2m, promo.Value == 0 ? cartTotal : promo.Value),
            _ => 0
        };

        return Math.Min(discount, cartTotal);
    }

    private async Task PublishPromoUsageAsync(PromoRedeemRequestDto request, decimal discount, PromoCode promo)
    {
        if (_kafkaProducer == null)
        {
            return;
        }

        var evt = new PromoUsageEvent
        {
            PromoCode = promo.Code,
            UserId = request.UserId,
            OrderId = string.IsNullOrWhiteSpace(request.OrderId) ? Guid.NewGuid().ToString("N") : request.OrderId,
            DiscountAmount = Math.Round(discount, 2, MidpointRounding.AwayFromZero),
            Currency = promo.Currency,
            UsedAt = DateTime.UtcNow
        };

        await _kafkaProducer.ProduceAsync("promo-events", promo.Code, evt);
    }
}
