using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AetherRemoteCommon.V2.Domain.Network.GetToken;
using AetherRemoteServer.Domain;
using AetherRemoteServer.Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace AetherRemoteServer.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(Configuration config, IDatabaseService db) : ControllerBase
{
    private readonly Version _expectedVersion = new(2, 3, 0, 1);

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] GetTokenRequest request)
    {
        if (request.Version != _expectedVersion)
            return BadRequest("Version Mismatch");

        var friendCode = await db.GetFriendCodeBySecret(request.Secret);
        if (friendCode is null)
            return Unauthorized("You are not registered");

        var token = GenerateJwtToken(
        [
            new Claim(AuthClaimTypes.FriendCode, friendCode)
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