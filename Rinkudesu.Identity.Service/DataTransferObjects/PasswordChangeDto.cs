using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace Rinkudesu.Identity.Service.DataTransferObjects;

[ExcludeFromCodeCoverage]
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
