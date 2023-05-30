using System.ComponentModel.DataAnnotations;

namespace Rinkudesu.Identity.Service.DataTransferObjects;

/// <summary>
/// Data requited for account registration.
/// </summary>
public class RegisterAccountDto
{
    /// <summary>
    /// Email of the new user.
    /// </summary>
    [Required, DataType(DataType.EmailAddress)]
    public string Email { get; set; } = string.Empty;
    /// <summary>
    /// Password of the new user.
    /// </summary>
    [Required]
    public string Password { get; set; } = string.Empty;
    /// <summary>
    /// <see cref="Password"/> repeated.
    /// </summary>
    [Required]
    public string PasswordRepeat { get; set; } = string.Empty;

    /// <summary>
    /// Indicates that there's a password mismatch between <see cref="Password"/> and <see cref="PasswordRepeat"/>.
    /// </summary>
    public bool PasswordMismatch => Password != PasswordRepeat;
}
