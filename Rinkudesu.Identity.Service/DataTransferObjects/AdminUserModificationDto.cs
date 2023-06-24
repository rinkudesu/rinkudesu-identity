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
}
