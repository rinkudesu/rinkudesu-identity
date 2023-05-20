using Rinkudesu.Identity.Service.Models;

namespace Rinkudesu.Identity.Service.Utilities;

public static class HttpContextExtensions
{
    public static UserInfo GetUser(this HttpContext context) => context.Items["user"] as UserInfo ?? throw new InvalidOperationException("User is missing from context items");
}