using System.ComponentModel.DataAnnotations;

namespace Rinkudesu.Identity.Service.DataTransferObjects;

public class ResetPasswordDto
{
    [Required]
    public Guid UserId { get; set; }
    [Required]
    public string Token { get; set; } = string.Empty;
    [Required]
    public string Password { get; set; } = string.Empty;
    [Required]
    public string PasswordRepeat { get; set; } = string.Empty;

    public bool PasswordMismatch => Password != PasswordRepeat;
}
