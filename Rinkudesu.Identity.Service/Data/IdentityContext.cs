#pragma warning disable CS1591
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Rinkudesu.Identity.Service.Models;

namespace Rinkudesu.Identity.Service.Data;

public class IdentityContext : IdentityDbContext<User, Role, Guid>
{
    public IdentityContext(DbContextOptions<IdentityContext> options) : base(options)
    {
    }
    //todo: some of the identity storage columns should have types changed from text to something actually reasonable
}
