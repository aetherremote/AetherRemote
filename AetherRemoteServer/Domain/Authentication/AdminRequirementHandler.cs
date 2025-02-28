using AetherRemoteServer.Authentication;
using AetherRemoteServer.Authentication.Requirements;
using AetherRemoteServer.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace AetherRemoteServer.Domain.Authentication;

/// <summary>
///     Represents the requirements to call admin functions on the Hub
/// </summary>
public class AdminRequirementHandler(DatabaseService db, ILogger<AdminRequirementHandler> logger)
    : AuthorizationHandler<AdminRequirement, HubInvocationContext>
{
    /// <summary>
    ///     Handle if the user submitting the request is an admin or not
    /// </summary>
    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context,
        AdminRequirement requirement, HubInvocationContext resource)
    {
        var friendCode = context.User.Claims.SingleOrDefault(claim =>
            string.Equals(claim.Type, AuthClaimTypes.FriendCode, StringComparison.Ordinal))?.Value;

        if (friendCode is null)
        {
            logger.LogWarning("Friend code claim was not found");
            context.Fail();
            return;
        }

        var user = await db.GetUserByFriendCode(friendCode);
        if (user is null)
        {
            logger.LogWarning("User was not found for {FriendCode}", friendCode);
            context.Fail();
            return;
        }

        if (user.IsAdmin is false)
        {
            logger.LogWarning("User was not admin for {FriendCode}", friendCode);
            context.Fail();
            return;
        }

        context.Succeed(requirement);
    }
}