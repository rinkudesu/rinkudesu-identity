using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using Rinkudesu.Identity.Service.Models;
using Rinkudesu.Identity.Service.Repositories;
using Rinkudesu.Identity.Service.Utilities;

namespace Rinkudesu.Identity.Service.Controllers;

/// <summary>
/// Controller responsible for JWT management, providing endpoints required for issuing and verifying tokens.
/// </summary>
[ApiController]
[ApiVersion("v1"), Route("api/[controller]"), Route("api/v{version:apiVersion}/[controller]")]
public class JwtController : ControllerBase
{
    private readonly UserManager<User> _userManager;
    private readonly JwtKeysRepository _jwtKeysRepository;
    private readonly JwtSecurityTokenHandler _tokenHandler;

    private static readonly JsonSerializerOptions jwksSerializerOptions = new JsonSerializerOptions
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    /// <inheritdoc />
    public JwtController(UserManager<User> userManager, JwtKeysRepository jwtKeysRepository, JwtSecurityTokenHandler tokenHandler)
    {
        _userManager = userManager;
        _jwtKeysRepository = jwtKeysRepository;
        _tokenHandler = tokenHandler;
    }

    /// <summary>
    /// Returns a openid-connect compliant list of Json Web Keys used for JWT signing. The response is in json format.
    /// </summary>
    [HttpGet("jwks.json")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public ActionResult Jwsk()
    {
        var keysJson = $"{{\"keys\":[{JsonSerializer.Serialize(_jwtKeysRepository.GetRsaAsJsonWebKey(), jwksSerializerOptions)}]}}";
        return Content(keysJson,"application/json");
    }

    /// <summary>
    /// Generates a new JWT for the current user. The token is valid for 10 minutes.
    /// </summary>
    /// <returns>A valid JWT returned as string.</returns>
    /// <remarks>
    /// This token should never really be presented to the user directly. It should only be used for back-end authorisation.
    /// For front-end tokens, the identity cookie should be used.
    /// </remarks>
    /// <response code="404">When user is not set or cannot be found.</response>
    [HttpGet, Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> IssueJwt()
    {
        if (User.Identity?.Name is null)
            return NotFound();
        var user = await _userManager.FindByNameAsync(User.Identity.Name);
        if (user is null)
            return NotFound();

        var claims = new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
        });

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = claims,
            Expires = DateTime.UtcNow.AddMinutes(10),
            Audience = "rinkudesu",
            Issuer = EnvironmentalVariablesReader.GetBaseUrl(),
            SigningCredentials = _jwtKeysRepository.GetRsaAsSigningCredentials(),
        };

        var token = _tokenHandler.CreateToken(tokenDescriptor);
        var tokenString = _tokenHandler.WriteToken(token);
        return Ok(tokenString);
    }
}
