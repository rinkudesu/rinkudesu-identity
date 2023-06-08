using Microsoft.EntityFrameworkCore;
using Rinkudesu.Identity.Service.Data;
using Rinkudesu.Identity.Service.DataTransferObjects;
using Rinkudesu.Identity.Service.DataTransferObjects.QueryModels;
using Rinkudesu.Identity.Service.Models;

namespace Rinkudesu.Identity.Service.Repositories;

/// <summary>
/// This is a repository for easy handling of database queries for the <see cref="User"/> object.
/// If you want to manage user accounts (create, remove, edit,...) use the <see cref="Microsoft.AspNetCore.Identity.UserManager{T}"/> class.
/// </summary>
public class UserAccountsRepository
{
    private readonly IdentityContext _context;

    /// <summary>
    /// Creates new repository instance.
    /// </summary>
    public UserAccountsRepository(IdentityContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Returns a list of existing users in DTO format.
    /// </summary>
    public async Task<List<UserAdminDetailsDto>> GetUsers(AccountAdminQueryModel queryModel)
    {
        var usersQuery = _context.Users.AsNoTracking().Select(u => new UserAdminDetailsDto
        {
            Id = u.Id,
            Email = u.Email!,
            EmailNormalised = u.NormalizedEmail!,
            EmailConfirmed = u.EmailConfirmed,
            TwoFactorEnabled = u.TwoFactorEnabled,
            AccountLockedOut = u.LockoutEnd != null,
            IsAdmin = _context.UserRoles.Any(ur => ur.UserId == u.Id && ur.RoleId == Role.Roles.Admin.GetRoleId()),
        });
        usersQuery = queryModel.ApplyModel(usersQuery);
        return await usersQuery.ToListAsync();
    }
}
