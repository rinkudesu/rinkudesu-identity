using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using Rinkudesu.Identity.Service.Models;

namespace Rinkudesu.Identity.Service.Controllers;

[ApiController, Route("api/[controller]")]
public class JwtController : ControllerBase
{
    private readonly UserManager<User> _userManager;
    private readonly JwtSecurityTokenHandler _tokenHandler = new JwtSecurityTokenHandler();

    //todo: this is so temp it physically hurts
    private static JsonWebKey? jsonWebKey;
    private static RSA rsa;

    public static JsonWebKey JWK {
        get
        {
            if (jsonWebKey is not null)
                return jsonWebKey;
            rsa = RSA.Create(2048);
            jsonWebKey = new JsonWebKey
            {
                KeyId = Guid.NewGuid().ToString(),
                Use = "sig",
                Alg = "RSA256",
                Kty = "RSA",
                E = Base64UrlEncoder.Encode(rsa.ExportParameters(false).Exponent),
                N = Base64UrlEncoder.Encode(rsa.ExportParameters(false).Modulus),
            };
            return jsonWebKey;
        }
    }

    public JwtController(UserManager<User> userManager)
    {
        _userManager = userManager;
    }

    [HttpGet("jwks.json")]
    public ActionResult Jwsk()
    {
        return Content($"{{\"keys\":[{System.Text.Json.JsonSerializer.Serialize(JWK, new JsonSerializerOptions{DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull, PropertyNamingPolicy = JsonNamingPolicy.CamelCase})}]}}","application/json");
    }

    [HttpGet, Authorize]
    public async Task<ActionResult> IssueJwt()
    {
        var user = await _userManager.FindByNameAsync(User.Identity.Name);
        var key = "move this thing to a config file you baka"u8.ToArray();

        var claims = new ClaimsIdentity(new Claim[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
        });

        _ = JWK;
        var sk = new RsaSecurityKey(rsa);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = claims,
            Expires = DateTime.UtcNow.AddMinutes(10),
            Audience = "rinkudesu",
            Issuer = "http://localhost:5500/",
            SigningCredentials = new SigningCredentials(sk, SecurityAlgorithms.RsaSha256),
        };

        var token = _tokenHandler.CreateToken(tokenDescriptor);
        var tokenString = _tokenHandler.WriteToken(token);
        return Ok(tokenString);
    }
}
