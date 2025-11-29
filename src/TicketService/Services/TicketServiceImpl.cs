using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using QRCoder;
using Shared.Events;
using Shared.Kafka;
using TicketService.Data;
using TicketService.DTOs;
using TicketService.Models;

namespace TicketService.Services;

public class TicketServiceImpl : ITicketService
{
    private readonly TicketDbContext _context;
    private readonly KafkaProducer? _kafkaProducer;
    private readonly IConfiguration _configuration;
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);

    public TicketServiceImpl(TicketDbContext context, KafkaProducer? kafkaProducer, IConfiguration configuration)
    {
        _context = context;
        _kafkaProducer = kafkaProducer;
        _configuration = configuration;
    }

    public async Task<Ticket> PurchaseTicketAsync(PurchaseTicketDto dto, string userId)
    {
        SeatLock? seatLock = null;
        if (!string.IsNullOrWhiteSpace(dto.SeatLockToken))
        {
            seatLock = await _context.SeatLocks.FirstOrDefaultAsync(s => s.LockToken == dto.SeatLockToken, CancellationToken.None);
            if (seatLock == null || seatLock.Status != SeatLockStatus.Held || seatLock.ExpiresAt <= DateTime.UtcNow)
            {
                throw new InvalidOperationException("Koltuk kilidi geçersiz veya süresi dolmuş");
            }

            if (!seatLock.UserId.Equals(userId, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("Bu kilit size ait değil");
            }
        }

        var ticketCode = GenerateTicketCode();
        var qrCodeImage = GenerateQRCode(ticketCode);

        var ticket = new Ticket
        {
            EventId = dto.EventId,
            UserId = userId,
            TicketCode = ticketCode,
            Price = dto.Price,
            QRCodeImage = qrCodeImage,
            PurchasedAt = DateTime.UtcNow,
            Status = TicketStatus.Active,
            SeatCode = seatLock?.SeatCode ?? dto.SeatCode,
            SeatLockToken = seatLock?.LockToken
        };

        if (seatLock != null)
        {
            seatLock.Status = SeatLockStatus.Committed;
            seatLock.UpdatedAt = DateTime.UtcNow;
        }

        _context.Tickets.Add(ticket);
        await _context.SaveChangesAsync();

        if (_kafkaProducer != null)
        {
            var ticketCreatedEvent = new TicketCreatedEvent
            {
                TicketId = ticket.Id,
                UserId = ticket.UserId,
                EventId = ticket.EventId,
                Price = ticket.Price,
                TicketCode = ticket.TicketCode,
                PurchasedAt = ticket.PurchasedAt
            };

            await _kafkaProducer.ProduceAsync("ticket-created", ticket.TicketCode, ticketCreatedEvent);
        }

        return ticket;
    }

    public async Task<List<Ticket>> GetUserTicketsAsync(string userId)
    {
        return await _context.Tickets
            .Where(t => t.UserId == userId)
            .OrderByDescending(t => t.PurchasedAt)
            .ToListAsync();
    }

    public async Task<Ticket?> GetTicketByCodeAsync(string ticketCode)
    {
        return await _context.Tickets
            .FirstOrDefaultAsync(t => t.TicketCode == ticketCode);
    }

    public async Task<bool> ValidateAndUseTicketAsync(string ticketCode)
    {
        var ticket = await GetTicketByCodeAsync(ticketCode);

        if (ticket == null || ticket.Status != TicketStatus.Active)
            return false;

        ticket.Status = TicketStatus.Used;
        ticket.UsedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<byte[]> GetTicketQRCodeAsync(string ticketCode)
    {
        var ticket = await GetTicketByCodeAsync(ticketCode);

        if (ticket?.QRCodeImage == null)
            throw new InvalidOperationException("Bilet bulunamadı veya QR kod mevcut değil");

        return ticket.QRCodeImage;
    }

    public async Task<QrVerificationResponse> VerifyQrAsync(QrVerificationRequest request)
    {
        try
        {
            var payloadJson = Encoding.UTF8.GetString(Convert.FromBase64String(request.Payload));
            var payload = JsonSerializer.Deserialize<QrPayload>(payloadJson, SerializerOptions);
            if (payload == null)
            {
                return InvalidResponse("Payload çözümlenemedi");
            }

            if (IsExpired(payload.IssuedAt))
            {
                return InvalidResponse("QR kodunun süresi dolmuş");
            }

            if (!VerifySignature(payload))
            {
                return InvalidResponse("İmza doğrulanamadı");
            }

            var ticket = await GetTicketByCodeAsync(payload.TicketId);
            if (ticket == null)
            {
                return InvalidResponse("Bilet bulunamadı");
            }

            if (!string.IsNullOrWhiteSpace(ticket.SeatCode) && !string.Equals(ticket.SeatCode, payload.SeatCode, StringComparison.OrdinalIgnoreCase))
            {
                return InvalidResponse("Koltuk eşleşmedi");
            }

            if (ticket.Status == TicketStatus.Cancelled)
            {
                return InvalidResponse("Bilet iptal edilmiş");
            }

            if (ticket.Status == TicketStatus.Used)
            {
                return new QrVerificationResponse
                {
                    IsValid = false,
                    Message = "Bilet daha önce kullanılmış",
                    TicketCode = ticket.TicketCode,
                    TicketStatus = ticket.Status
                };
            }

            ticket.Status = TicketStatus.Used;
            ticket.UsedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return new QrVerificationResponse
            {
                IsValid = true,
                Message = "Bilet doğrulandı",
                TicketCode = ticket.TicketCode,
                TicketStatus = ticket.Status
            };
        }
        catch (FormatException)
        {
            return InvalidResponse("Geçersiz payload");
        }
    }

    public async Task<SeatLockResponseDto> LockSeatsAsync(string userId, SeatLockRequestDto dto)
    {
        var normalizedSeats = dto.SeatCodes.Select(s => s.Trim().ToUpperInvariant()).Distinct().ToList();
        await CleanupExpiredLocksAsync(dto.EventId);

        var now = DateTime.UtcNow;
        var conflicts = await _context.SeatLocks
            .Where(s => s.EventId == dto.EventId && normalizedSeats.Contains(s.SeatCode) && s.Status == SeatLockStatus.Held && s.ExpiresAt > now)
            .Select(s => s.SeatCode)
            .ToListAsync();

        if (conflicts.Any())
        {
            throw new InvalidOperationException($"Kilitli koltuklar: {string.Join(",", conflicts)}");
        }

        var lockToken = Guid.NewGuid().ToString("N");
        var expiresAt = now.AddSeconds(dto.HoldSeconds);

        var locks = normalizedSeats.Select(seat => new SeatLock
        {
            EventId = dto.EventId,
            SeatCode = seat,
            UserId = userId,
            LockToken = lockToken,
            ExpiresAt = expiresAt,
            Status = SeatLockStatus.Held,
            CreatedAt = now,
            UpdatedAt = now
        }).ToList();

        _context.SeatLocks.AddRange(locks);
        await _context.SaveChangesAsync();
        await PublishSeatEventsAsync(locks);

        return new SeatLockResponseDto
        {
            LockToken = lockToken,
            ExpiresAt = expiresAt,
            SeatCodes = normalizedSeats
        };
    }

    public async Task<bool> ReleaseLockAsync(string userId, string lockToken)
    {
        var locks = await _context.SeatLocks.Where(s => s.LockToken == lockToken && s.UserId == userId && s.Status == SeatLockStatus.Held).ToListAsync();
        if (!locks.Any())
        {
            return false;
        }

        foreach (var seatLock in locks)
        {
            seatLock.Status = SeatLockStatus.Expired;
            seatLock.ExpiresAt = DateTime.UtcNow;
            seatLock.UpdatedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();
        await PublishSeatEventsAsync(locks);
        return true;
    }

    public async Task<bool> CommitLockAsync(string lockToken, SeatCommitRequestDto dto)
    {
        var locks = await _context.SeatLocks.Where(s => s.LockToken == lockToken && s.Status == SeatLockStatus.Held).ToListAsync();
        if (!locks.Any())
        {
            return false;
        }

        var now = DateTime.UtcNow;
        if (locks.Any(l => l.ExpiresAt <= now))
        {
            foreach (var expired in locks)
            {
                expired.Status = SeatLockStatus.Expired;
                expired.UpdatedAt = now;
            }

            await _context.SaveChangesAsync();
            return false;
        }

        foreach (var seatLock in locks)
        {
            seatLock.Status = SeatLockStatus.Committed;
            seatLock.UpdatedAt = now;
        }

        await _context.SaveChangesAsync();
        await PublishSeatEventsAsync(locks);
        return true;
    }

    public async Task<TicketCancellationResponseDto?> RequestCancellationAsync(int ticketId, string userId, TicketCancellationRequestDto dto)
    {
        var ticket = await _context.Tickets.FirstOrDefaultAsync(t => t.Id == ticketId);
        if (ticket == null || !ticket.UserId.Equals(userId, StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        if (ticket.CancellationStatus == TicketCancellationStatus.Pending)
        {
            throw new InvalidOperationException("Bu bilet için iptal talebi zaten mevcut");
        }

        ticket.CancellationStatus = TicketCancellationStatus.Pending;
        ticket.CancellationRequestedAt = DateTime.UtcNow;

        var request = new TicketCancellationRequest
        {
            TicketId = ticket.Id,
            UserId = userId,
            Reason = dto.Reason,
            Decision = CancellationDecision.Pending,
            CreatedAt = DateTime.UtcNow
        };

        _context.CancellationRequests.Add(request);
        await _context.SaveChangesAsync();
        await PublishCancellationEventAsync(ticket, request);

        return new TicketCancellationResponseDto
        {
            RequestId = request.Id,
            Decision = request.Decision,
            RefundAmount = request.RefundAmount,
            RefundCurrency = request.RefundCurrency
        };
    }

    private static bool IsExpired(long issuedAt)
    {
        var issued = DateTimeOffset.FromUnixTimeSeconds(issuedAt);
        return issued < DateTimeOffset.UtcNow.AddMinutes(-5);
    }

    private bool VerifySignature(QrPayload payload)
    {
        var secret = _configuration["Qr:SigningKey"] ?? "dev-qr-secret";
        var canonical = $"{payload.TicketId}:{payload.EventId}:{payload.UserId}:{payload.SeatCode}:{payload.IssuedAt}";
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret));
        var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(canonical));

        try
        {
            var provided = Convert.FromBase64String(payload.Signature);
            return provided.Length == hash.Length && CryptographicOperations.FixedTimeEquals(provided, hash);
        }
        catch (FormatException)
        {
            return false;
        }
    }

    private async Task CleanupExpiredLocksAsync(string eventId)
    {
        var now = DateTime.UtcNow;
        var expiredLocks = await _context.SeatLocks.Where(s => s.EventId == eventId && s.Status == SeatLockStatus.Held && s.ExpiresAt <= now).ToListAsync();
        if (!expiredLocks.Any())
        {
            return;
        }

        foreach (var seatLock in expiredLocks)
        {
            seatLock.Status = SeatLockStatus.Expired;
            seatLock.UpdatedAt = now;
        }

        await _context.SaveChangesAsync();
    }

    private string GenerateTicketCode()
    {
        return $"TKT-{Guid.NewGuid().ToString("N")[..12].ToUpper()}";
    }

    private byte[] GenerateQRCode(string ticketCode)
    {
        using var qrGenerator = new QRCodeGenerator();
        using var qrCodeData = qrGenerator.CreateQrCode(ticketCode, QRCodeGenerator.ECCLevel.Q);
        using var qrCode = new PngByteQRCode(qrCodeData);
        return qrCode.GetGraphic(20);
    }

    private static QrVerificationResponse InvalidResponse(string message) => new()
    {
        IsValid = false,
        Message = message
    };

    private sealed record QrPayload
    {
        public string TicketId { get; init; } = string.Empty;
        public string EventId { get; init; } = string.Empty;
        public string UserId { get; init; } = string.Empty;
        public string? SeatCode { get; init; }
        public long IssuedAt { get; init; }
        public string Signature { get; init; } = string.Empty;
    }

    private async Task PublishSeatEventsAsync(IEnumerable<SeatLock> seatLocks)
    {
        if (_kafkaProducer == null)
        {
            return;
        }

        foreach (var seatLock in seatLocks)
        {
            var evt = new SeatStatusChangedEvent
            {
                EventId = seatLock.EventId,
                SeatCode = seatLock.SeatCode,
                Status = seatLock.Status.ToString(),
                UserId = seatLock.UserId,
                LockToken = seatLock.LockToken,
                Timestamp = DateTime.UtcNow
            };

            await _kafkaProducer.ProduceAsync("seat-updates", seatLock.SeatCode, evt);
        }
    }

    private async Task PublishCancellationEventAsync(Ticket ticket, TicketCancellationRequest request)
    {
        if (_kafkaProducer == null)
        {
            return;
        }

        var evt = new TicketCancellationRequestedEvent
        {
            TicketId = ticket.Id,
            UserId = ticket.UserId,
            EventId = ticket.EventId,
            Reason = request.Reason,
            RequestId = request.Id.ToString(),
            RequestedAt = request.CreatedAt
        };

        await _kafkaProducer.ProduceAsync("ticket-cancellations", ticket.TicketCode, evt);
    }
}
