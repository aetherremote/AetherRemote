using System.Security.Claims;
using System.Text;
using AetherRemoteCommon;
using AetherRemoteCommon.Network.Domain.Api;
using AetherRemoteCommon.Network.Enums.ErrorCodes;
using AetherRemoteServer.Domain;
using AetherRemoteServer.Infrastructure.Database;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace AetherRemoteServer.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(Configuration config, DatabaseInfrastructure database) : ControllerBase
{
    // Const
    private static readonly Version ExpectedVersion = new(2, 10, 3, 2);
    
    // Instantiated
    private readonly SymmetricSecurityKey _key = new(Encoding.UTF8.GetBytes(config.SigningKey));

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] GetTokenRequest request)
    {
        if (request.Version < ExpectedVersion)
            return StatusCode(StatusCodes.Status409Conflict, new GetTokenResponse(GetTokenEc.VersionMismatch, null));
        
        if (await database.GetFriendCodeBySecret(request.Secret) is not { } friendCode)
            return StatusCode(StatusCodes.Status401Unauthorized, new GetTokenResponse(GetTokenEc.UnknownSecret, null));

        var token = GenerateJwtToken([new Claim(AuthClaimTypes.FriendCode, friendCode)]);

        return StatusCode(StatusCodes.Status200OK, new GetTokenResponse(GetTokenEc.Success, token));
    }
    
    private string GenerateJwtToken(IEnumerable<Claim> claims)
    {
        var descriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            SigningCredentials = new SigningCredentials(_key, SecurityAlgorithms.HmacSha256),
            Expires = DateTime.UtcNow.AddHours(Constraints.TokenExpirationInHours)
        };

        return new JsonWebTokenHandler().CreateToken(descriptor);
    }
}
