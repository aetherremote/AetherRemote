using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AetherRemoteCommon.Domain.Enums;
using AetherRemoteCommon.Domain.Network.GetToken;
using AetherRemoteCommon.Domain.Network.LoginAuthentication;
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
    private static readonly Version ExpectedVersion = new(2, 8,4, 0);
    
    // Instantiated
    private readonly SymmetricSecurityKey _key = new(Encoding.UTF8.GetBytes(config.SigningKey));

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] GetTokenRequest request)
    {
        if (request.Version != ExpectedVersion)
            return StatusCode(StatusCodes.Status409Conflict, new LoginAuthenticationResult(LoginAuthenticationErrorCode.VersionMismatch));
        
        if (await database.GetFriendCodeBySecret(request.Secret) is not { } friendCode)
            return StatusCode(StatusCodes.Status401Unauthorized, new LoginAuthenticationResult(LoginAuthenticationErrorCode.UnknownSecret));

        var token = GenerateJwtToken([new Claim(AuthClaimTypes.FriendCode, friendCode)]);

        return StatusCode(StatusCodes.Status200OK, new LoginAuthenticationResult(LoginAuthenticationErrorCode.Success, token.RawData));
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