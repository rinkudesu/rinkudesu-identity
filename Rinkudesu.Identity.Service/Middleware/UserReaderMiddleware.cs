using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Identity;
using Rinkudesu.Identity.Service.Models;

namespace Rinkudesu.Identity.Service.Middleware;

[ExcludeFromCodeCoverage]
public class UserReaderMiddleware
{
    private readonly RequestDelegate _next;

    public UserReaderMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, UserManager<User> userManager)
    {
        if ((context.User.Identity?.IsAuthenticated ?? false) && !string.IsNullOrEmpty(context.User.Identity.Name))
        {
            var user = await userManager.FindByNameAsync(context.User.Identity.Name);
            if (user is null)
            {
                context.Response.StatusCode = 404;
                return;
            }
            context.Items["user"] = new UserInfo(user);
        }
        await _next(context);
    }
}
