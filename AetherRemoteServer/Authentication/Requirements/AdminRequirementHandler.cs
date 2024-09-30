using AetherRemoteServer.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace AetherRemoteServer.Authentication.Requirements;

public class AdminRequirementHandler(DatabaseService db, ILogger<AdminRequirementHandler> logger) : AuthorizationHandler<AdminRequirement, HubInvocationContext>
{
    // Injected
    private readonly DatabaseService db = db;
    private readonly ILogger<AdminRequirementHandler> logger = logger;

    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, AdminRequirement requirement, HubInvocationContext resource)
    {
        var friendCode = context.User.Claims.SingleOrDefault(claim => string.Equals(claim.Type, AuthClaimTypes.FriendCode, StringComparison.Ordinal))?.Value;
        if (friendCode == null) { logger.LogWarning("Fail! FriendCode null"); context.Fail(); return; }

        var user = await db.GetUser(friendCode);
        if (user == null) { logger.LogWarning("Fail! User null"); context.Fail(); return; }

        if (user.Value.IsAdmin == false) { logger.LogWarning("Fail! Not admin!"); context.Fail(); return; }

        context.Succeed(requirement);
    }
}
