using System.Diagnostics.CodeAnalysis;
using Rinkudesu.Identity.Service.Models;
#pragma warning disable CS1591

namespace Rinkudesu.Identity.Service.Utilities;

public static class HttpContextExtensions
{
    /// <summary>
    /// Reads currently logged in user from context items.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when no user is available in context items, indicating that the user is not logged in.</exception>
    [ExcludeFromCodeCoverage]
    public static UserInfo GetUser(this HttpContext context) => context.Items["user"] as UserInfo ?? throw new InvalidOperationException("User is missing from context items");
}
