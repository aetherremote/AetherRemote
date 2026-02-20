using AetherRemoteCommon.Domain.Enums;
using AetherRemoteCommon.Domain.Network;
using AetherRemoteCommon.Domain.Network.RemoveFriend;
using AetherRemoteCommon.Domain.Network.SyncOnlineStatus;
using Microsoft.AspNetCore.SignalR;

namespace AetherRemoteServer.SignalR.Handlers.Test;

public partial class RequestHandler
{
    /// <summary>
    ///     Handles the logic for fulfilling a <see cref="RemoveFriendRequest"/>
    /// </summary>
    public async Task<RemoveFriendResponse> HandleRemoveFriend(string senderFriendCode, RemoveFriendRequest request, IHubCallerClients clients)
    {
        var result = await _databaseService.DeletePermissions(senderFriendCode, request.TargetFriendCode) switch
        {
            DatabaseResultEc.NoOp => RemoveFriendEc.NotFriends,
            DatabaseResultEc.Success => RemoveFriendEc.Success,
            _ => RemoveFriendEc.Unknown
        };

        // If the request wasn't meaningful
        if (result is not RemoveFriendEc.Success)
            return new RemoveFriendResponse(result);
        
        // If the target isn't online
        if (_presenceService.TryGet(request.TargetFriendCode) is not { } friend)
            return new RemoveFriendResponse(result);
        
        // If the target is online, but they don't have us added
        if (await _databaseService.GetSinglePermissions(request.TargetFriendCode, senderFriendCode) is null)
            return new RemoveFriendResponse(result);
        
        try
        {
            // Send a message to say our status goes from online to pending
            var forward = new SyncOnlineStatusCommand(senderFriendCode, FriendOnlineStatus.Pending, null);
            await clients.Client(friend.ConnectionId).SendAsync(HubMethod.SyncOnlineStatus, forward);
        }
        catch (Exception e)
        {
            _logger.LogError("Syncing online status {Sender} -> {Target} failed, {Error}", senderFriendCode, request.TargetFriendCode, e);
        }
        
        // Return always
        return new RemoveFriendResponse(result);
    }
}