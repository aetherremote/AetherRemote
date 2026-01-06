using AetherRemoteCommon.Domain.Enums;
using AetherRemoteCommon.Domain.Network;
using AetherRemoteCommon.Domain.Network.RemoveFriend;
using AetherRemoteCommon.Domain.Network.SyncOnlineStatus;
using AetherRemoteServer.Domain.Interfaces;
using Microsoft.AspNetCore.SignalR;

namespace AetherRemoteServer.SignalR.Handlers;

/// <summary>
///     Handles the logic for fulfilling a <see cref="RemoveFriendRequest"/>
/// </summary>
public class RemoveFriendHandler(IConnectionsService connections, IDatabaseService databaseService, ILogger<RemoveFriendHandler> logger)
{
    /// <summary>
    ///     Handles the request
    /// </summary>
    public async Task<RemoveFriendResponse> Handle(string senderFriendCode, RemoveFriendRequest request, IHubCallerClients clients)
    {
        var result = await databaseService.DeletePermissions(senderFriendCode, request.TargetFriendCode) switch
        {
            DatabaseResultEc.NoOp => RemoveFriendEc.NotFriends,
            DatabaseResultEc.Success => RemoveFriendEc.Success,
            _ => RemoveFriendEc.Unknown
        };

        // If the request wasn't meaningful
        if (result is not RemoveFriendEc.Success)
            return new RemoveFriendResponse(result);
        
        // If the target isn't online
        if (connections.TryGetClient(request.TargetFriendCode) is not { } friend)
            return new RemoveFriendResponse(result);
        
        // If the target is online, but they don't have us added
        if (await databaseService.GetPermissions(request.TargetFriendCode, senderFriendCode) is null)
            return new RemoveFriendResponse(result);
        
        try
        {
            // Send a message to say our status goes from online to pending
            var forward = new SyncOnlineStatusForwardedRequest(senderFriendCode, FriendOnlineStatus.Pending, null);
            await clients.Client(friend.ConnectionId).SendAsync(HubMethod.SyncOnlineStatus, forward);
        }
        catch (Exception e)
        {
            logger.LogError("Syncing online status {Sender} -> {Target} failed, {Error}", senderFriendCode, request.TargetFriendCode, e);
        }
        
        // Return always
        return new RemoveFriendResponse(result);
    }
}