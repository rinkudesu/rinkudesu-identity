using System.Diagnostics.CodeAnalysis;

namespace Rinkudesu.Identity.Service.DataTransferObjects;

/// <summary>
/// Class containing changes to the user entity requested by an administrator.
/// </summary>
/// <remarks>
/// For every nullable field, null means "no change".
/// </remarks>
[ExcludeFromCodeCoverage]
public class AdminUserModificationDto
{
    /// <summary>
    /// Indicates whether the user should be granted admin rights.
    /// </summary>
    public bool? Admin { get; set; }

    /// <summary>
    /// Indicates whether the user account should be locked or unlocked.
    /// </summary>
    public bool? Locked { get; set; }

    /// <summary>
    /// Indicates whether the email should be set as confirmed or not.
    /// </summary>
    public bool? EmailConfirmed { get; set; }
}
