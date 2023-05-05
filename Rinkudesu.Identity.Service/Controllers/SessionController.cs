using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Rinkudesu.Identity.Service.Models;

namespace Rinkudesu.Identity.Service.Controllers;

[ApiController, Route("api/[controller]")]
public class SessionController : ControllerBase
{
    private readonly UserManager<User> _userManager;
    private readonly SignInManager<User> _signInManager;

    public SessionController(UserManager<User> userManager, SignInManager<User> signInManager)
    {
        _userManager = userManager;
        _signInManager = signInManager;
    }

    [HttpPost("wang")]
    public async Task<ActionResult> Wang()
    {
        var user = new User { UserName = "test" };
        await _userManager.CreateAsync(user, "1qazXSW@");
        return Ok();
    }

    [HttpPost("login")]
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

    [HttpPost("logout")]
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
