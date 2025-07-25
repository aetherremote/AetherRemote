using AetherRemoteCommon.Domain.Network;
using AetherRemoteCommon.Domain.Network.SyncOnlineStatus;
using AetherRemoteServer.Domain;
using AetherRemoteServer.Domain.Interfaces;
using Microsoft.AspNetCore.SignalR;

namespace AetherRemoteServer.SignalR.Handlers;

/// <summary>
///     Processes clients connecting and disconnecting from the server
/// </summary>
public class OnlineStatusUpdateHandler(
    IConnectionsService connections,
    IDatabaseService database,
    ILogger<OnlineStatusUpdateHandler> logger)
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

        var friendPermissions = await database.GetPermissions(friendCode);
        foreach (var (target, permissions) in friendPermissions.Permissions)
        {
            if (connections.TryGetClient(target) is not { } friendInfo)
                continue;

            var request = new SyncOnlineStatusForwardedRequest(friendCode, online, online ? permissions : null);

            try
            {
                await clients.Client(friendInfo.ConnectionId).SendAsync(HubMethod.SyncOnlineStatus, request);
            }
            catch (Exception e)
            {
                logger.LogWarning("Unable to sync {FriendCode}'s online status to {Target}, {Exception}", friendCode,
                    target, e.Message);
            }
        }
    }
}