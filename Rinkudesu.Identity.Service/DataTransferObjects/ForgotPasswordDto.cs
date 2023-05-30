using System.ComponentModel.DataAnnotations;

namespace Rinkudesu.Identity.Service.DataTransferObjects;

/// <summary>
/// Data requesting forgot password email for a user.
/// </summary>
public class ForgotPasswordDto
{
    /// <summary>
    /// Email associated with the account requesting password reset.
    /// </summary>
    [Required]
    public string Email { get; set; } = string.Empty;
}
