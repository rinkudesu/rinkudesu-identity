using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Rinkudesu.Identity.Service.Models;

namespace Rinkudesu.Identity.Service.Controllers;

/// <summary>
/// Manages current user session.
/// </summary>
/// <remarks>
/// NOTE: in order to manage multiple user sessions (ie. log user out everywhere), see <see cref="AccountManagementController"/>.
/// </remarks>
[ApiController]
[ApiVersion("1"), Route("api/[controller]"), Route("api/v{version:apiVersion}/[controller]")]
public class SessionController : ControllerBase
{
    private readonly UserManager<User> _userManager;
    private readonly SignInManager<User> _signInManager;

    /// <inheritdoc />
    public SessionController(UserManager<User> userManager, SignInManager<User> signInManager)
    {
        _userManager = userManager;
        _signInManager = signInManager;
    }

    /// <summary>
    /// Logs user in with a new session.
    /// </summary>
    /// <param name="userName">Username/email of the user logging in.</param>
    /// <param name="password">Password of the user logging in.</param>
    /// <response code="200">
    /// When user was logged in correctly.
    /// Note that the session token will be sent as a cookie.
    /// </response>
    /// <response code="404">Send when user doesn't exist or the password didn't match.</response>
    [HttpPost("login")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> LogIn([FromForm] string userName, [FromForm] string password)
    {
        var user = await _userManager.FindByNameAsync(userName);
        if (user is null)
            return NotFound();
        var loginResult = await _signInManager.PasswordSignInAsync(user, password, false, true);
        if (!loginResult.Succeeded)
            return NotFound();
        return Ok();
    }

    /// <summary>
    /// Logs user out of current session.
    /// </summary>
    /// <response code="200">When the user was logged out or wasn't logged in in the first place.</response>
    [HttpPost("logout")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult> LogOut()
    {
        if (!User.Identity?.IsAuthenticated ?? true)
            return Ok();

        //so apparently _signInManager.SignOutAsync doesn't actually invalidate the session id
        //only when HttpContext method is used, is it deleted from session store
        await _signInManager.SignOutAsync();
        await HttpContext.SignOutAsync();
        return Ok();
    }
}
