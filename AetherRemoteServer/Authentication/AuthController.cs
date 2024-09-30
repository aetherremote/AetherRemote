using AetherRemoteServer.Domain;
using AetherRemoteServer.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace AetherRemoteServer.Authentication;

[ApiController]
[Route("api/[controller]")]
public class AuthController(ILogger<AuthController> logger, ServerConfiguration config, DatabaseService db) : ControllerBase
{
    // Inject
    private readonly ILogger<AuthController> logger = logger;
    private readonly ServerConfiguration config = config;
    private readonly DatabaseService db = db;

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] string secret)
    {
        var user = await db.GetUser(secret, DatabaseService.QueryUserType.Secret);
        if (user == null) return Unauthorized("You are not registered");

        var token = GenerateJwtToken(
        [
            new(AuthClaimTypes.FriendCode, user.Value.FriendCode)
        ]);

        return Ok(token.RawData);
    }

    private JwtSecurityToken GenerateJwtToken(List<Claim> claims)
    {
        // TODO: Retrieve these details from a config
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config.SigningKey));
        var token = new SecurityTokenDescriptor()
        {
            Subject = new ClaimsIdentity(claims),
            SigningCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256Signature),
            Expires = DateTime.UtcNow.AddHours(4)
        };

        var handler = new JwtSecurityTokenHandler();
        return handler.CreateJwtSecurityToken(token);
    }
}
