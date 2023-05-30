using System.ComponentModel.DataAnnotations;
using Rinkudesu.Identity.Service.Models;

namespace Rinkudesu.Identity.Service.DataTransferObjects;

/// <summary>
/// Data validating user email change with a token.
/// </summary>
public class ConfirmEmailChangeDto
{
    /// <summary>
    /// Id of the <see cref="User"/> to change email of.
    /// </summary>
    [Required]
    public Guid UserId { get; set; }
    /// <summary>
    /// Email verification token sent to the new address.
    /// </summary>
    [Required]
    public string Token { get; set; } = string.Empty;
    /// <summary>
    /// New address to set.
    /// </summary>
    [Required]
    public string NewEmail { get; set; } = string.Empty;
}
