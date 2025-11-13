using IdentityService.Models;

namespace IdentityService.Services;

public interface ITokenService
{
    string GenerateToken(ApplicationUser user);
}

