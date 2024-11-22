using AetherRemoteServer.Domain;
using AetherRemoteServer.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AetherRemoteCommon.Domain.Network;

namespace AetherRemoteServer.Authentication;

[ApiController]
[Route("api/[controller]")]
public class AuthController(ILogger<AuthController> logger, ServerConfiguration config, DatabaseService db) : ControllerBase
{
    private readonly Version _expectedVersion = new(1, 1, 0, 0);
    
    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        if (request.Version != _expectedVersion)
        {
            logger.LogInformation("Version: {Version}", request.Version);
            return BadRequest("Version Mismatch");
        }

        var user = await db.GetUser(request.Secret, DatabaseService.QueryUserType.Secret);
        if (user is null) return Unauthorized("You are not registered");

        var token = GenerateJwtToken(
        [
            new Claim(AuthClaimTypes.FriendCode, user.Value.FriendCode)
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
