using System.ComponentModel.DataAnnotations;

namespace Rinkudesu.Identity.Service.DataTransferObjects;

/// <summary>
/// Request for the change of user email.
/// </summary>
public class ChangeEmailDto
{
    /// <summary>
    /// The new email address to set.
    /// </summary>
    [Required]
    public string Email { get; set; } = string.Empty;
}
