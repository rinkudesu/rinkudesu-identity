using System.Diagnostics.CodeAnalysis;
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
    [ExcludeFromCodeCoverage]
    public UserAccountsRepository(IdentityContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Returns a list of existing users in DTO format.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public async Task<List<UserAdminDetailsDto>> GetUsers(AccountAdminQueryModel queryModel)
    {
        var usersQuery = GetUsersQuery();
        usersQuery = queryModel.ApplyModel(usersQuery);
        return await usersQuery.ToListAsync();
    }

    /// <summary>
    /// Returns a user with given id, if exists.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public async Task<UserAdminDetailsDto?> GetUser(Guid userId, CancellationToken cancellationToken = default)
        => await GetUsersQuery().FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

    private IQueryable<UserAdminDetailsDto> GetUsersQuery()
        => _context.Users.AsNoTracking().Select(u => new UserAdminDetailsDto
        {
            Id = u.Id,
            Email = u.Email!,
            EmailNormalised = u.NormalizedEmail!,
            EmailConfirmed = u.EmailConfirmed,
            TwoFactorEnabled = u.TwoFactorEnabled,
            AccountLockedOut = u.LockoutEnd != null,
            IsAdmin = _context.UserRoles.Any(ur => ur.UserId == u.Id && ur.RoleId == Role.Roles.Admin.GetRoleId()),
        });
}
