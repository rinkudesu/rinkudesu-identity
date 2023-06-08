using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Rinkudesu.Identity.Service.DataTransferObjects.QueryModels;
using Rinkudesu.Identity.Service.Models;
using Rinkudesu.Identity.Service.Repositories;

namespace Rinkudesu.Identity.Service.Controllers;

/// <summary>
/// Controller responsible for administrative management of all user accounts and permissions.
/// </summary>
[ApiController, Authorize(Roles = Role.RoleNames.Admin)]
[ApiVersion("1"), Route("api/[controller]"), Route("api/v{version:apiVersion}/[controller]")]
public class AccountAdminController : ControllerBase
{
    private readonly UserAccountsRepository _accountsRepository;

    /// <inheritdoc/>
    public AccountAdminController(UserAccountsRepository accountsRepository)
    {
        _accountsRepository = accountsRepository;
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
}
