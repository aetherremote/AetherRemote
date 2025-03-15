using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AetherRemoteCommon.Domain.Network;
using AetherRemoteServer.Authentication;
using AetherRemoteServer.Domain;
using AetherRemoteServer.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace AetherRemoteServer.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(Configuration config, DatabaseService db)
    : ControllerBase
{
    private readonly Version _expectedVersion = new(2, 0, 1, 0);

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] FetchTokenRequest request)
    {
        if (request.Version != _expectedVersion)
            return BadRequest("Version Mismatch");

        var user = await db.GetUserBySecret(request.Secret);
        if (user is null)
            return Unauthorized("You are not registered");
        
        var token = GenerateJwtToken(
        [
            new Claim(AuthClaimTypes.FriendCode, user.FriendCode)
        ]);

        return Ok(token.RawData);
    }

    private JwtSecurityToken GenerateJwtToken(List<Claim> claims)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config.SigningKey));
        var token = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            SigningCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256Signature),
            Expires = DateTime.UtcNow.AddHours(4)
        };

        return new JwtSecurityTokenHandler().CreateJwtSecurityToken(token);
    }
}