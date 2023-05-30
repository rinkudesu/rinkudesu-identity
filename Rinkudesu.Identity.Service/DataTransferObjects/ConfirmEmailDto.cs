using System.ComponentModel.DataAnnotations;
using Rinkudesu.Identity.Service.Models;

namespace Rinkudesu.Identity.Service.DataTransferObjects;

/// <summary>
/// Data confirming the user email when creating a new account.
/// </summary>
public class ConfirmEmailDto
{
    /// <summary>
    /// User id of the newly created <see cref="User"/>.
    /// </summary>
    [Required]
    public Guid UserId { get; set; }
    /// <summary>
    /// Confirmation token sent to the email provided when registering.
    /// </summary>
    [Required]
    public string EmailToken { get; set; } = string.Empty;
}
