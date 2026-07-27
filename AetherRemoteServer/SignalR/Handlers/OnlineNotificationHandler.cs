using AetherRemoteCommon.Domain.Enums;
using AetherRemoteCommon.Domain.Network;
using AetherRemoteCommon.Network.Domain;
using AetherRemoteCommon.Network.Domain.Payloads;
using AetherRemoteCommon.Util;
using AetherRemoteServer.Infrastructure.Database;
using AetherRemoteServer.Services;
using Microsoft.AspNetCore.SignalR;

namespace AetherRemoteServer.SignalR.Handlers;

public class OnlineNotificationHandler(
    ILogger<OnlineNotificationHandler> logger, 
    DatabaseInfrastructure databaseInfrastructure, 
    SessionService sessionService)
{
    public async Task Notify(string senderFriendCode, bool online, IHubCallerClients clients)
    {
        var senderGlobalPermissions = await databaseInfrastructure.GetGlobalPermissions(senderFriendCode);
        var senderAllPermissionPairs = await databaseInfrastructure.GetAllPermissions(senderFriendCode);
        foreach (var permissionPair in senderAllPermissionPairs)
        {
            if (permissionPair.PermissionsGrantedBy is null) // Skip pending friends
                continue; 
            
            if (sessionService.GetSession(permissionPair.TargetFriendCode) is not { } session) // Skip offline friends
                continue;
            
            try
            {
                var resolved = PermissionResolver.Resolve(senderGlobalPermissions, permissionPair.PermissionsGrantedTo);
                var payload = new SyncOnlineStatusPayload(online ? FriendOnlineStatus.Online : FriendOnlineStatus.Offline, resolved);
                var message = new Message<SyncOnlineStatusPayload>(senderFriendCode, payload);
                await clients.Client(session.ConnectionId).SendAsync(HubMethod.SyncOnlineStatus, message);
            }
            catch (Exception e)
            {
                logger.LogError("Syncing online status {Sender} -> {Target} failed, {Error}", senderFriendCode, permissionPair.TargetFriendCode, e);
            }
        }
    }
}