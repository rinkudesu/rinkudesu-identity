using System.Diagnostics.CodeAnalysis;
using System.Security.Claims;

namespace Rinkudesu.Identity.Service.Common.Utilities;

public static class ClaimsPrincipalExtensions
{
    [ExcludeFromCodeCoverage]
    [SuppressMessage("Design", "CA1062:Validate arguments of public methods")]
    public static Guid GetUserId(this ClaimsPrincipal principal)
        => Guid.Parse(principal.Claims.First(c => c.Type.EndsWith("nameidentifier", StringComparison.OrdinalIgnoreCase)).Value);
}
