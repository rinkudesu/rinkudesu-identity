using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace Rinkudesu.Identity.Service.Models;

/// <inheritdoc/>
public class Role : IdentityRole<Guid>
{
    [MaxLength(256)]
    public override string? ConcurrencyStamp { get; set; }
}
