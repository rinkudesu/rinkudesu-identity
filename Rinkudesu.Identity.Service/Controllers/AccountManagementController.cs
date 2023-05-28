using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.EntityFrameworkCore;
using Rinkudesu.Identity.Service.Data;
using Rinkudesu.Identity.Service.DataTransferObjects;
using Rinkudesu.Identity.Service.MessageQueues;
using Rinkudesu.Identity.Service.Models;
using Rinkudesu.Identity.Service.Repositories;
using Rinkudesu.Identity.Service.Utilities;
using Rinkudesu.Kafka.Dotnet.Base;

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
    public ActionResult<User> GetUserDetails()
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

    [HttpPost("logOutEverywhere")]
    public async Task<ActionResult> LogOutEverywhere([FromServices] SessionTicketRepository ticketRepository)
    {
        var user = HttpContext.GetUser();
        await _userManager.UpdateSecurityStampAsync(user.User);
        await ticketRepository.RemoveUserSessionTickets(user.User.Id);
        return Ok();
    }

    [HttpPost("deleteAccount")]
    public async Task<ActionResult> DeleteAccount([FromBody] DeleteAccountDto deleteAccountDto, [FromServices] IKafkaProducer kafkaProducer, [FromServices] SessionTicketRepository sessionTicketRepository)
    {
        var user = HttpContext.GetUser();

        // user must provide valid password before the account is deleted
        var passwordCheck = await _userManager.CheckPasswordAsync(user.User, deleteAccountDto.Password);
        if (!passwordCheck)
        {
            _logger.LogWarning("Failed user {UserId} password check before account deletion", user.User.Id.ToString());
            return NotFound();
        }

        var result = await _userManager.DeleteAsync(user.User);
        //send kafka message regardless, to at least remove all user data
        await sessionTicketRepository.RemoveUserSessionTickets(user.User.Id);
        await kafkaProducer.ProduceUserDeleted(user.User.Id);

        if (!result.Succeeded)
        {
            _logger.LogWarning("Failed to delete user {UserId} account", user.User.Id.ToString());
            return BadRequest();
        }
        return Ok();
    }

    [HttpPost("createAccount"), AllowAnonymous]
    public async Task<ActionResult> CreateAccount([FromBody] RegisterAccountDto accountDto)
    {
        if (!ModelState.IsValid || accountDto.PasswordMismatch)
            return BadRequest();

        var user = Models.User.CreateWithEmail(accountDto.Email);
        var result = await _userManager.CreateAsync(user, accountDto.Password);
        if (!result.Succeeded)
        {
            var reason = string.Join(", ", result.Errors.Select(e => e.Description));
            _logger.LogWarning("Failed to create account for email {Email} because {Reason}", accountDto.Email, reason);
            return BadRequest(reason);
        }

        return CreatedAtAction(nameof(CreateAccount), new AccountCreatedDto
        {
            EmailConfirmationToken = await _userManager.GenerateEmailConfirmationTokenAsync(user),
            UserId = user.Id,
        });
    }

    [HttpPost("confirmEmail"), AllowAnonymous]
    public async Task<ActionResult> ConfirmEmail([FromBody] ConfirmEmailDto confirmEmailDto)
    {
        if (!ModelState.IsValid)
            return BadRequest();

        var user = await _userManager.FindByIdAsync(confirmEmailDto.UserId.ToString());
        if (user is null)
            return NotFound();

        var result = await _userManager.ConfirmEmailAsync(user, confirmEmailDto.EmailToken);
        if (!result.Succeeded)
            return NotFound();
        return Ok();
    }

    [HttpPost("forgotPassword"), AllowAnonymous]
    public async Task<ActionResult> ForgotPassword([FromBody] ForgotPasswordDto model)
    {
        if (!ModelState.IsValid)
            return BadRequest();

        var user = await _userManager.FindByEmailAsync(model.Email);
        if (user is null)
            return NotFound();

        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        return Ok(new PasswordRecoveryDto
        {
            UserId = user.Id,
            Token = token,
        });
    }

    [HttpPost("resetPassword"), AllowAnonymous]
    public async Task<ActionResult> ResetPassword([FromBody] ResetPasswordDto model)
    {
        if (!ModelState.IsValid || model.PasswordMismatch)
            return BadRequest();

        var user = await _userManager.FindByIdAsync(model.UserId.ToString());
        if (user is null)
            return NotFound();

        var result = await _userManager.ResetPasswordAsync(user, model.Token, model.Password);
        if (!result.Succeeded)
        {
            var reason = string.Join(", ", result.Errors.Select(e => e.Description));
            _logger.LogWarning("Failed to reset password of user {UserId} because {Reason}", model.UserId.ToString(), reason);
            return BadRequest(reason);
        }
        return Ok();
    }

    [HttpPost("changeEmail")]
    public async Task<ActionResult> ChangeEmail([FromBody] ChangeEmailDto model)
    {
        if (!ModelState.IsValid)
            return BadRequest();

        var user = HttpContext.GetUser();
        if (model.Email == user.User.Email)
            return BadRequest("Provided email is the same as before");

        var token = await _userManager.GenerateChangeEmailTokenAsync(user.User, model.Email);
        return Ok(new EmailChangeConfirmationDto(user.User.Id, model.Email, token));
    }

    [HttpPost("confirmEmailChange")]
    public async Task<ActionResult> ConfirmEmailChange([FromBody] ConfirmEmailChangeDto model, [FromServices] IdentityContext context, [FromServices] SessionTicketRepository ticketRepository)
    {
        if (!ModelState.IsValid || model.UserId != HttpContext.GetUser().User.Id)
            return BadRequest();

        var user = HttpContext.GetUser();
        var executionStrategy = context.Database.CreateExecutionStrategy();

        async Task<IdentityResult> changeUsername(User actionUser, ConfirmEmailChangeDto actionModel, UserManager<User> actionManager)
        {
            var result = await actionManager.ChangeEmailAsync(actionUser, actionModel.NewEmail, actionModel.Token);
            // change without explicitly aborting the transaction as this changed nothing so far and may contain valuable errors
            if (!result.Succeeded)
                return result;
            if (!(await actionManager.SetUserNameAsync(actionUser, actionModel.NewEmail)).Succeeded)
                throw new DbUpdateException("Failed to change username");
            return result;
        }
        async Task<bool> verifyChanged(User actionUser, ConfirmEmailChangeDto actionModel, UserManager<User> actionManager)
        {
            var dbUser = await actionManager.FindByIdAsync(actionUser.Id.ToString());
            return dbUser is not null && dbUser.Email == actionModel.NewEmail && dbUser.UserName == actionModel.NewEmail;
        }
        var result = await executionStrategy.ExecuteInTransactionAsync<IdentityResult>(_ => changeUsername(user.User, model, _userManager), _ =>  verifyChanged(user.User, model, _userManager));

        if (!result.Succeeded)
        {
            var reason = string.Join(", ", result.Errors.Select(e => e.Description));
            _logger.LogWarning("Failed to change email for user {UserId} because {Reason}", user.User.Id.ToString(), reason);
            return BadRequest(reason);
        }
        // force the user to log in again to refresh email in session tickets
        await ticketRepository.RemoveUserSessionTickets(user.User.Id);
        return Ok();
    }
}
