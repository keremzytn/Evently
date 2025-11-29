using Microsoft.AspNetCore.Mvc;
using PaymentService.DTOs;
using PaymentService.Services;

namespace PaymentService.Controllers;

[ApiController]
[Route("api/promos")]
public class PromoCodesController : ControllerBase
{
    private readonly IPromoCodeService _promoService;

    public PromoCodesController(IPromoCodeService promoService)
    {
        _promoService = promoService;
    }

    [HttpPost("validate")]
    public async Task<IActionResult> Validate([FromBody] PromoValidationRequestDto request)
    {
        OverrideUserId(request);
        var result = await _promoService.ValidateAsync(request, ResolveClientIp());
        if (!result.IsValid)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    [HttpPost("redeem")]
    public async Task<IActionResult> Redeem([FromBody] PromoRedeemRequestDto request)
    {
        OverrideUserId(request);
        var result = await _promoService.RedeemAsync(request, ResolveClientIp());
        if (!result.IsValid)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    [HttpGet("catalog")]
    public async Task<IActionResult> Catalog()
    {
        var catalog = await _promoService.GetCatalogAsync();
        return Ok(catalog);
    }

    [HttpGet("logs")]
    public async Task<IActionResult> Logs([FromQuery] int limit = 100)
    {
        var logs = await _promoService.GetLogsAsync(Math.Clamp(limit, 1, 500));
        return Ok(logs);
    }

    private void OverrideUserId(PromoValidationRequestDto request)
    {
        var headerId = Request.Headers["X-User-Id"].ToString();
        if (!string.IsNullOrWhiteSpace(headerId))
        {
            request.UserId = headerId;
        }
    }

    private string ResolveClientIp()
    {
        return HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
    }
}
