using AetherRemoteCommon.Domain.Enums;
using AetherRemoteCommon.Domain.Network;
using AetherRemoteCommon.Network.Domain;
using AetherRemoteCommon.Network.Domain.Payloads;
using AetherRemoteCommon.Util;
using AetherRemoteServer.Infrastructure.Database;
using AetherRemoteServer.Services;
using Microsoft.AspNetCore.SignalR;

namespace AetherRemoteServer.SignalR.Handlers;

public class OnlineStatusUpdateHandler(
    ILogger<OnlineStatusUpdateHandler> logger, 
    DatabaseInfrastructure databaseInfrastructure, 
    PresenceService presenceService)
{
    public async Task HandleOnlineStatusUpdate(string friendCode, bool online, IHubCallerClients clients)
    {
        // TODO: Manage possession events like removing a session
        if (online is false)
            presenceService.Remove(friendCode);
        
        var global = await databaseInfrastructure.GetGlobalPermissions(friendCode);
        var permissions = await databaseInfrastructure.GetAllPermissions(friendCode);
        foreach (var permission in permissions)
        {
            if (permission.PermissionsGrantedBy is null) continue; // Pending Friend
            if (presenceService.TryGet(permission.TargetFriendCode) is not { } target)
                continue;

            try
            {
                if (online)
                {
                    var resolved = PermissionResolver.Resolve(global, permission.PermissionsGrantedTo);
                    var payload = new SyncOnlineStatusPayload(FriendOnlineStatus.Online, resolved);
                    var message = new Message<SyncOnlineStatusPayload>(friendCode, payload);
                    await clients.Client(target.ConnectionId).SendAsync(HubMethod.SyncOnlineStatus, message);
                }
                else
                {
                    var payload = new SyncOnlineStatusPayload(FriendOnlineStatus.Offline);
                    var message = new Message<SyncOnlineStatusPayload>(friendCode, payload);
                    await clients.Client(target.ConnectionId).SendAsync(HubMethod.SyncOnlineStatus, message);
                }
            }
            catch (Exception e)
            {
                logger.LogError("Syncing online status {Sender} -> {Target} failed, {Error}", friendCode, permission.TargetFriendCode, e);
            }
        }
    }
}