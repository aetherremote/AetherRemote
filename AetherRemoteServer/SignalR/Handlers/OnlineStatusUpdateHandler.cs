using AetherRemoteCommon.Domain.Network;
using AetherRemoteServer.Domain;
using AetherRemoteServer.Domain.Interfaces;
using Microsoft.AspNetCore.SignalR;

namespace AetherRemoteServer.SignalR.Handlers;

/// <summary>
///     TODO
/// </summary>
public class OnlineStatusUpdateHandler(
    IClientConnectionService connections,
    IDatabaseService database,
    ILogger<OnlineStatusUpdateHandler> logger)
{
    /// <summary>
    ///     TODO
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

            var request = new SyncOnlineStatusAction
            {
                SenderFriendCode = friendCode,
                Online = online,
                Permissions = online ? permissions : null
            };

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