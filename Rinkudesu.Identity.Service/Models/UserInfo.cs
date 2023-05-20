namespace Rinkudesu.Identity.Service.Models;

public class UserInfo
{
    public User User { get; }

    public UserInfo(User user)
    {
        User = user;
    }
}
