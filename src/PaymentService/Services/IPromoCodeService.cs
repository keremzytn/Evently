using PaymentService.DTOs;
using PaymentService.Models;

namespace PaymentService.Services;

public interface IPromoCodeService
{
    Task<PromoValidationResponseDto> ValidateAsync(PromoValidationRequestDto request, string clientIp);
    Task<PromoValidationResponseDto> RedeemAsync(PromoRedeemRequestDto request, string clientIp);
    Task<IReadOnlyCollection<PromoCode>> GetCatalogAsync();
    Task<IReadOnlyCollection<PromoValidationLog>> GetLogsAsync(int limit = 100);
}
