using System.ComponentModel.DataAnnotations;

namespace Rinkudesu.Identity.Service.DataTransferObjects;

public class PasswordChangeDto
{
    [Required]
    public string OldPassword { get; set; } = string.Empty;
    [Required]
    public string NewPassword { get; set; } = string.Empty;
    [Required]
    public string NewPasswordRepeat { get; set; } = string.Empty;

    public bool NewPasswordsMatch => NewPassword == NewPasswordRepeat;
}
