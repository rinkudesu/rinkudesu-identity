using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Rinkudesu.Identity.Service.DataTransferObjects;
using Rinkudesu.Identity.Service.DataTransferObjects.QueryModels;
using Rinkudesu.Identity.Service.Models;
using Rinkudesu.Identity.Service.Repositories;
using Rinkudesu.Identity.Service.Utilities;

namespace Rinkudesu.Identity.Service.Controllers;

/// <summary>
/// Controller responsible for administrative management of all user accounts and permissions.
/// </summary>
[ApiController, Authorize(Roles = Role.RoleNames.Admin)]
[ApiVersion("1"), Route("api/[controller]"), Route("api/v{version:apiVersion}/[controller]")]
public class AccountAdminController : ControllerBase
{
    private readonly UserAccountsRepository _accountsRepository;
    private readonly UserManager<User> _userManager;

    /// <inheritdoc/>
    public AccountAdminController(UserAccountsRepository accountsRepository, UserManager<User> userManager)
    {
        _accountsRepository = accountsRepository;
        _userManager = userManager;
    }

    /// <summary>
    /// Returns a list of users that have created an account.
    /// </summary>
    /// <response code="200">Returned when user is in a role allowing them to view the list of users.</response>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult> GetUsers([FromQuery] AccountAdminQueryModel queryModel)
    {
        var users = await _accountsRepository.GetUsers(queryModel);
        return Ok(users);
    }

    /// <summary>
    /// Creates a new user and returns details about them.
    /// </summary>
    /// <remarks>
    /// Users created this way will have emails automatically verified.
    /// If you wish to ensure email validity, leave the password empty, which will force the user to execute forgotten password flow.
    /// </remarks>
    /// <response code="200">Returned when user account was created correctly.</response>
    /// <response code="400">Returned when user account couldn't be created.</response>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> CreateUser([FromBody] AdminAccountCreateDto newAccount, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return BadRequest();

        var user = newAccount.MakeUser();
        user.EmailConfirmed = true;
        var result = string.IsNullOrWhiteSpace(newAccount.Password) ? _userManager.CreateAsync(user) : _userManager.CreateAsync(user, newAccount.Password);
        if (!(await result).Succeeded)
            return BadRequest();

        return CreatedAtAction(nameof(CreateUser), await _accountsRepository.GetUser(user.Id, cancellationToken));
    }
}
