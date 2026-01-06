using AetherRemoteCommon.Domain;
using AetherRemoteCommon.Domain.Enums;
using AetherRemoteCommon.Domain.Network;
using AetherRemoteCommon.Domain.Network.AddFriend;
using AetherRemoteCommon.Domain.Network.SyncOnlineStatus;
using AetherRemoteServer.Domain.Interfaces;
using Microsoft.AspNetCore.SignalR;

namespace AetherRemoteServer.SignalR.Handlers;

/// <summary>
///     Handles the logic for fulfilling a <see cref="AddFriendRequest"/>
/// </summary>
public class AddFriendHandler(IConnectionsService connections, IDatabaseService database, ILogger<AddFriendHandler> logger)
{
    /// <summary>
    ///     Handles the request
    /// </summary>
    public async Task<AddFriendResponse> Handle(string friendCode, AddFriendRequest request, IHubCallerClients clients)
    {
        // Create the permissions in the database
        var result = await database.CreatePermissions(friendCode, request.TargetFriendCode);
        
        // Map the result
        var code = result switch
        {
            DatabaseResultEc.Success => AddFriendEc.Success,
            DatabaseResultEc.Pending => AddFriendEc.Pending,
            DatabaseResultEc.AlreadyFriends => AddFriendEc.AlreadyFriends,
            DatabaseResultEc.NoSuchFriendCode => AddFriendEc.NoSuchFriendCode,
            _ => AddFriendEc.Unknown
        };
        
        // Only update other person if it is a success
        if (code is not AddFriendEc.Success)
        {
            return code is AddFriendEc.Pending 
                ? new AddFriendResponse(code, FriendOnlineStatus.Pending) 
                : new AddFriendResponse(code, FriendOnlineStatus.Offline);
        }

        // Only update if they are online
        if (connections.TryGetClient(request.TargetFriendCode) is not { } target)
            return new AddFriendResponse(code, FriendOnlineStatus.Offline);
        
        try
        {
            // Try to send an update to that client that we've accepted the friend request
            var sync = new SyncOnlineStatusForwardedRequest(friendCode, FriendOnlineStatus.Online, new UserPermissions());
            await clients.Client(target.ConnectionId).SendAsync(HubMethod.SyncOnlineStatus, sync);
        }
        catch (Exception e)
        {
            logger.LogError("Syncing online status {Sender} -> {Target} failed, {Error}", friendCode, request.TargetFriendCode, e);
        }
        
        return new AddFriendResponse(code, FriendOnlineStatus.Online);
    }
}