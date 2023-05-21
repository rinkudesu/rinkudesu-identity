using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Identity;

namespace Rinkudesu.Identity.Service.Models;

/// <inheritdoc/>
[ExcludeFromCodeCoverage]
public class Role : IdentityRole<Guid>
{
    [MaxLength(256)]
    public override string? ConcurrencyStamp { get; set; }
}
