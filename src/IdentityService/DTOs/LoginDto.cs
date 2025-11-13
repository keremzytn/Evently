using System.ComponentModel.DataAnnotations;

namespace IdentityService.DTOs;

public class LoginDto
{
    [Required(ErrorMessage = "Email gereklidir")]
    [EmailAddress(ErrorMessage = "Geçersiz email formatı")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Şifre gereklidir")]
    public string Password { get; set; } = string.Empty;
}

