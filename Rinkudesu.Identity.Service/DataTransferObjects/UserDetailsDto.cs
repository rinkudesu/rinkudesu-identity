using System.Diagnostics.CodeAnalysis;
using Rinkudesu.Identity.Service.Models;

namespace Rinkudesu.Identity.Service.DataTransferObjects;

/// <summary>
/// Returns details about a user account.
/// </summary>
[ExcludeFromCodeCoverage]
public class UserDetailsDto
{
    /// <summary>
    /// Current user name.
    /// </summary>
    public string UserName { get; set; }
    /// <summary>
    /// Current user email. Will be always the same as <see cref="UserName"/>.
    /// </summary>
    public string Email { get; set; }
    /// <summary>
    /// Indicates whether the user has second factor enabled or not.
    /// </summary>
    public bool TwoFactorEnabled { get; set; }

    /// <summary>
    /// Initialises the data based on <see cref="User"/> object.
    /// </summary>
    /// <param name="user"></param>
    public UserDetailsDto(User user)
    {
        UserName = user.UserName ?? string.Empty;
        Email = user.Email ?? string.Empty;
        TwoFactorEnabled = user.TwoFactorEnabled;
    }
}
