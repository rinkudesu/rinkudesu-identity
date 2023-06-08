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
    /// Applies current query model to a provided queryable of users.
    /// </summary>
    public IQueryable<UserAdminDetailsDto> ApplyModel(IQueryable<UserAdminDetailsDto> users)
    {
        users = Sort(users);
        users = SkipTake(users);

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
