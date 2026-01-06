using AetherRemoteCommon.Domain.Enums;
using AetherRemoteCommon.Domain.Network;
using AetherRemoteCommon.Domain.Network.SyncOnlineStatus;
using AetherRemoteServer.Domain;
using AetherRemoteServer.Domain.Interfaces;
using Microsoft.AspNetCore.SignalR;

namespace AetherRemoteServer.SignalR.Handlers;

/// <summary>
///     Processes clients connecting and disconnecting from the server
/// </summary>
public class OnlineStatusUpdateHandler(IConnectionsService connections, IDatabaseService database, ILogger<OnlineStatusUpdateHandler> logger)
{
    /// <summary>
    ///     Handle the event, removing or adding from the current client list, and updating all the user's friends who are online
    /// </summary>
    public async Task Handle(string friendCode, bool online, HubCallerContext context, IHubCallerClients clients)
    {
        if (online)
            connections.TryAddClient(friendCode, new ClientInfo(context.ConnectionId));
        else
            connections.TryRemoveClient(friendCode);

        var permissions = await database.GetAllPermissions(friendCode);
        foreach (var permission in permissions)
        {
            // Ignore pending friends
            if (permission.PermissionsGrantedBy is null)
                continue;

            // Only evaluate online friends
            if (connections.TryGetClient(permission.TargetFriendCode) is not { } friend)
                continue;

            try
            {
                if (online)
                {
                    var request = new SyncOnlineStatusForwardedRequest(friendCode, FriendOnlineStatus.Online, permission.PermissionsGrantedTo);
                    await clients.Client(friend.ConnectionId).SendAsync(HubMethod.SyncOnlineStatus, request);
                }
                else
                {
                    var request = new SyncOnlineStatusForwardedRequest(friendCode, FriendOnlineStatus.Offline, null);
                    await clients.Client(friend.ConnectionId).SendAsync(HubMethod.SyncOnlineStatus, request);
                }
            }
            catch (Exception e)
            {
                logger.LogError("Syncing online status {Sender} -> {Target} failed, {Error}", friendCode, permission.TargetFriendCode, e);
            }
        }
    }
}