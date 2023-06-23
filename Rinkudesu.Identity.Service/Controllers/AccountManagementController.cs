using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Rinkudesu.Identity.Service.Data;
using Rinkudesu.Identity.Service.DataTransferObjects;
using Rinkudesu.Identity.Service.Models;
using Rinkudesu.Identity.Service.Repositories;
using Rinkudesu.Identity.Service.Services;
using Rinkudesu.Identity.Service.Utilities;

namespace Rinkudesu.Identity.Service.Controllers;

/// <summary>
/// Controller responsible for managing user account and settings.
/// Unless otherwise mentioned, the user must be logged in to access this controller.
/// </summary>
[ApiController, Authorize]
[ApiVersion("1"), Route("api/[controller]"), Route("api/v{version:apiVersion}/[controller]")]
public class AccountManagementController : ControllerBase
{
    private readonly UserManager<User> _userManager;
    private readonly ILogger<AccountManagementController> _logger;

    /// <inheritdoc />
    public AccountManagementController(UserManager<User> userManager, ILogger<AccountManagementController> logger)
    {
        _userManager = userManager;
        _logger = logger;
    }

    /// <summary>
    /// Returns user details for currently logged-in user.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public ActionResult<User> GetUserDetails()
    {
        var data = new UserDetailsDto(HttpContext.GetUser().User);
        return Ok(data);
    }

    /// <summary>
    /// Changes password of currently logged-in user.
    /// The password must meet the minimum password requirements and both password and repeat password must match.
    /// </summary>
    /// <response code="400">
    /// Returned when request model is invalid (ie. passwords don't match) or password change failed.
    /// In the second case, an additional error string will be returned indicating why the change has failed.
    /// </response>
    [HttpPost("changePassword")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
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

    /// <summary>
    /// Logs the current user out of all sessions.
    /// </summary>
    [HttpPost("logOutEverywhere")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult> LogOutEverywhere([FromServices] SessionTicketRepository ticketRepository)
    {
        var user = HttpContext.GetUser();
        await _userManager.UpdateSecurityStampAsync(user.User);
        await ticketRepository.RemoveUserSessionTickets(user.User.Id);
        return Ok();
    }

    /// <summary>
    /// Deletes the current user account and sends a message for other microservices to remove all related data.
    /// </summary>
    /// <response code="200">Returned when the account was successfully deleted.</response>
    /// <response code="400">Returned when the account failed to be deleted.</response>
    /// <response code="404">Returned when password provided did not match.</response>
    [HttpPost("deleteAccount")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> DeleteAccount([FromBody] DeleteAccountDto deleteAccountDto, [FromServices] UserRemover userRemover)
    {
        var user = HttpContext.GetUser();

        // user must provide valid password before the account is deleted
        var passwordCheck = await _userManager.CheckPasswordAsync(user.User, deleteAccountDto.Password);
        if (!passwordCheck)
        {
            _logger.LogWarning("Failed user {UserId} password check before account deletion", user.User.Id.ToString());
            return NotFound();
        }

        var result = await userRemover.RemoveUser(user.User);

        if (!result)
        {
            _logger.LogWarning("Failed to delete user {UserId} account", user.User.Id.ToString());
            return BadRequest();
        }
        return Ok();
    }

    /// <summary>
    /// Creates a new user account, but does not log the user in.
    /// This method allows access without being logged in.
    /// </summary>
    /// <remarks>
    /// Please note that if email confirmation is enabled, the user will first need to confirm the email address.
    /// Otherwise, the login will fail.
    /// </remarks>
    /// <response code="201">Returns new user id and email confirmation token, if the user account was successfully created.</response>
    /// <response code="400">
    /// Returned when request model is invalid (ie. passwords don't match) or account creation has failed.
    /// In the second case, an additional error string will be returned indicating why the change has failed.
    /// </response>
    [HttpPost("createAccount"), AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> CreateAccount([FromBody] RegisterAccountDto accountDto)
    {
        if (!ModelState.IsValid || accountDto.PasswordMismatch)
            return BadRequest();

        var user = Models.User.CreateWithEmail(accountDto.Email);
        var result = await _userManager.CreateAsync(user, accountDto.Password);
        if (!result.Succeeded)
        {
            var existing = await _userManager.FindByEmailAsync(accountDto.Email);
            if (existing is not null)
                return BadRequest("Email already exists");

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

    /// <summary>
    /// Confirms user email using confirmation token.
    /// This method allows access without being logged in.
    /// </summary>
    /// <response code="200">Returned when email confirmation was successful.</response>
    /// <response code="400">Returned when request model was invalid.</response>
    /// <response code="404">Returned when either the user id or token couldn't be verified.</response>
    [HttpPost("confirmEmail"), AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
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

    /// <summary>
    /// Requests forgot password recovery token.
    /// This method allows access without being logged in.
    /// </summary>
    /// <remarks>
    /// Please note that in order for this flow to be secure, there should be no difference in the front-end between the email not being found and the email being correctly sent.
    /// </remarks>
    /// <response code="200">Email was valid and recovery token was generated.</response>
    /// <response code="400">Request model was invalid.</response>
    /// <response code="404">No user with this email address is registered.</response>
    [HttpPost("forgotPassword"), AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
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

    /// <summary>
    /// Resets the password using the email recovery token. This method allows access without being logged in.
    /// </summary>
    /// <response code="200">Returned when the password was correctly set.</response>
    /// <response code="400">
    /// Returned when the model was invalid or the password was unable to be set.
    /// In the latter case, an error string is returned indicating what went wrong.
    /// </response>
    /// <response code="404">Returned when user with given id was not found.</response>
    [HttpPost("resetPassword"), AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
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

    /// <summary>
    /// Requests user email change.
    /// Note that this doesn't actually change the email, but just sends a confirmation to the provided address.
    /// The user must first click the confirmation link in order to actually change the email.
    /// </summary>
    /// <response code="200">Returns email confirmation data required for sending the email.</response>
    /// <response code="400">Returns when request model is not valid.</response>
    [HttpPost("changeEmail")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
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

    /// <summary>
    /// Changes user email if the provided token is valid.
    /// </summary>
    /// <remarks>
    /// Note that this method will also change the username, as we treat them as one and the same.
    /// </remarks>
    /// <response code="200">Returned when the email/username was changed correctly.</response>
    /// <response code="400">Returned when the change couldn't be made. Might contain error string with more details.</response>
    [HttpPost("confirmEmailChange")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
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
