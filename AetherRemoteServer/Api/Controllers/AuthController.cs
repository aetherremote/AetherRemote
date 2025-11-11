using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AetherRemoteCommon.Domain.Network.GetToken;
using AetherRemoteServer.Domain;
using AetherRemoteServer.Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace AetherRemoteServer.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(Configuration config, IDatabaseService database) : ControllerBase
{
    // Const
    private static readonly Version ExpectedVersion = new(2, 7,0, 1);
    
    // Instantiated
    private readonly SymmetricSecurityKey _key = new(Encoding.UTF8.GetBytes(config.SigningKey));

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] GetTokenRequest request)
    {
        if (request.Version != ExpectedVersion)
            return BadRequest("Version Mismatch");

        var friendCode = await database.GetFriendCodeBySecret(request.Secret);
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
        var token = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            SigningCredentials = new SigningCredentials(_key, SecurityAlgorithms.HmacSha256Signature),
            Expires = DateTime.UtcNow.AddHours(4)
        };

        return new JwtSecurityTokenHandler().CreateJwtSecurityToken(token);
    }
}