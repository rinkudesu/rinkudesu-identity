using System.Diagnostics.CodeAnalysis;

namespace Rinkudesu.Identity.Service.Models;

[ExcludeFromCodeCoverage]
public class UserInfo
{
    public User User { get; }

    public UserInfo(User user)
    {
        User = user;
    }
}
