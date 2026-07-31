using AetherRemoteServer.Domain.Attributes;
using Microsoft.AspNetCore.Mvc;

namespace AetherRemoteServer.Api.Controllers;

[ApiController]
[LocalHostOnly]
[Route("internal/[controller]")]
public class DiscordController : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Register(object request)
    {
        return Ok();
    }
}