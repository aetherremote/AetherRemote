using AetherRemoteCommon.Domain.Enums;
using AetherRemoteCommon.Domain.Network;
using AetherRemoteCommon.Domain.Network.SyncOnlineStatus;
using AetherRemoteServer.Domain.Interfaces;
using AetherRemoteServer.SignalR.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace AetherRemoteServer.SignalR.Handlers;

/// <summary>
///     Processes clients connecting and disconnecting from the server
/// </summary>
public class OnlineStatusUpdateHandler(
    IDatabaseService database,
    IPresenceService presenceService,
    IHubContext<PrimaryHub> hub,
    ILogger<OnlineStatusUpdateHandler> logger)
{
    /// <summary>
    ///     Handle the event, removing or adding from the current client list, and updating all the user's friends who are online
    /// </summary>
    public async Task Handle(string friendCode, bool online)
    {
        if (online is false)
            presenceService.Remove(friendCode);

        var permissions = await database.GetAllPermissions(friendCode);
        foreach (var permission in permissions)
        {
            // Ignore pending friends
            if (permission.PermissionsGrantedBy is null)
                continue;
            
            // Only evaluate online friends
            if (presenceService.TryGet(permission.TargetFriendCode) is null)
                continue;

            try
            {
                if (online)
                {
                    var request = new SyncOnlineStatusCommand(friendCode, FriendOnlineStatus.Online, permission.PermissionsGrantedTo);
                    await hub.Clients.Group(permission.TargetFriendCode).SendAsync(HubMethod.SyncOnlineStatus, request);
                }
                else
                {
                    var request = new SyncOnlineStatusCommand(friendCode, FriendOnlineStatus.Offline, null);
                    await hub.Clients.Group(permission.TargetFriendCode).SendAsync(HubMethod.SyncOnlineStatus, request);
                }
            }
            catch (Exception e)
            {
                logger.LogError("Syncing online status {Sender} -> {Target} failed, {Error}", friendCode, permission.TargetFriendCode, e);
            }
        }
    }
}