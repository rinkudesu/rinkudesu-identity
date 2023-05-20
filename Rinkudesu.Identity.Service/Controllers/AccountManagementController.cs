using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Rinkudesu.Identity.Service.DataTransferObjects;
using Rinkudesu.Identity.Service.Models;
using Rinkudesu.Identity.Service.Utilities;

namespace Rinkudesu.Identity.Service.Controllers;

[ApiController, Authorize]
[ApiVersion("1"), Route("api/[controller]"), Route("api/v{version:apiVersion}/[controller]")]
public class AccountManagementController : ControllerBase
{
    private readonly UserManager<User> _userManager;
    private readonly ILogger<AccountManagementController> _logger;

    public AccountManagementController(UserManager<User> userManager, ILogger<AccountManagementController> logger)
    {
        _userManager = userManager;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<User>> GetUserDetails()
    {
        var data = new UserDetailsDto(HttpContext.GetUser().User);
        return Ok(data);
    }

    [HttpPost("changePassword")]
    public async Task<ActionResult> ChangePassword([FromBody] PasswordChangeDto passwordChange)
    {
        if (!ModelState.IsValid || !passwordChange.NewPasswordsMatch)
            return BadRequest();
        var user = HttpContext.GetUser();
        _logger.LogInformation("User {UserId} attempting password change", user.User.Id.ToString());

        var result = await _userManager.ChangePasswordAsync(user.User, passwordChange.OldPassword, passwordChange.NewPassword);
        if (result.Succeeded)
            return Ok();

        var reason = string.Join(", ", result.Errors.Select(e => e.Description));
        _logger.LogWarning("Password change for user {UserId} failed due to {Reason}", user.User.Id.ToString(), reason);
        return BadRequest(reason);
    }
}
