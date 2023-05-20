using System.Diagnostics.CodeAnalysis;
using Rinkudesu.Identity.Service.Models;

namespace Rinkudesu.Identity.Service.DataTransferObjects;

[ExcludeFromCodeCoverage]
public class UserDetailsDto
{
    public string UserName { get; set; }
    public string Email { get; set; }
    public bool TwoFactorEnabled { get; set; }
    
    public UserDetailsDto(User user)
    {
        UserName = user.UserName ?? string.Empty;
        Email = user.Email ?? string.Empty;
        TwoFactorEnabled = user.TwoFactorEnabled;
    }
}
