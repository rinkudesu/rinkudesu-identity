using System.ComponentModel.DataAnnotations;

namespace Rinkudesu.Identity.Service.DataTransferObjects.QueryModels;

/// <summary>
/// Defined query parameters for returning a list of user accounts.
/// </summary>
public class AccountAdminQueryModel
{
    /// <summary>
    /// Selects how users are sorted.
    /// </summary>
    public SortOptions SortOption { get; set; }
    /// <summary>
    /// Defines how many rows should be skipped. Useful for pagination.
    /// </summary>
    [Range(0, int.MaxValue)]
    public int? Skip { get; set; }
    /// <summary>
    /// Limits the number of rows returned. Useful for pagination.
    /// </summary>
    [Range(0, int.MaxValue)]
    public int? Take { get; set; }
    /// <summary>
    /// If not null, will return only email containing the phrase provided.
    /// </summary>
    public string? EmailContains { get; set; }
    /// <summary>
    /// If <c>true</c> will return only accounts with admin privileges.
    /// </summary>
    public bool IsAdmin { get; set; }
    /// <summary>
    /// If has value, will return only accounts with the selected email confirmation state.
    /// </summary>
    public bool? EmailConfirmed { get; set; }
    /// <summary>
    /// If <c>true</c> will return only accounts that are currently locked.
    /// </summary>
    public bool LockedOnly { get; set; }

    /// <summary>
    /// Applies current query model to a provided queryable of users.
    /// </summary>
    public IQueryable<UserAdminDetailsDto> ApplyModel(IQueryable<UserAdminDetailsDto> users)
    {
        users = ApplyFilters(users);
        users = Sort(users);
        users = SkipTake(users);

        return users;
    }

    private IQueryable<UserAdminDetailsDto> ApplyFilters(IQueryable<UserAdminDetailsDto> users)
    {
        if (!string.IsNullOrWhiteSpace(EmailContains))
        {
            users = users.Where(u => u.EmailNormalised.Contains(EmailContains.ToUpperInvariant()));
        }
        if (IsAdmin)
        {
            users = users.Where(u => u.IsAdmin);
        }
        if (EmailConfirmed.HasValue)
        {
            users = users.Where(u => u.EmailConfirmed == EmailConfirmed.Value);
        }
        if (LockedOnly)
        {
            users = users.Where(u => u.AccountLockedOut);
        }
        return users;
    }

    private IQueryable<UserAdminDetailsDto> Sort(IQueryable<UserAdminDetailsDto> users)
        => SortOption switch
        {
            SortOptions.ByEmail => users.OrderBy(u => u.Email),
            _ => throw new InvalidOperationException("Current SortOption is not valid"),
        };

    private IQueryable<UserAdminDetailsDto> SkipTake(IQueryable<UserAdminDetailsDto> users)
        => users.Skip(Skip ?? 0).Take(Take ?? 20);

    /// <summary>
    /// Provides options for user list sort order.
    /// </summary>
    public enum SortOptions
    {
        /// <summary>
        /// Will sort users alphabetically by email (A-Z order)
        /// </summary>
        ByEmail,
    }
}
