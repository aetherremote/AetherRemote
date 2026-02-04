using AetherRemoteCommon.Domain.Enums;
using AetherRemoteCommon.Domain.Network;
using AetherRemoteCommon.Domain.Network.SyncPermissions;
using AetherRemoteCommon.Domain.Network.UpdateGlobalPermissions;
using AetherRemoteCommon.Util;
using AetherRemoteServer.Services;
using AetherRemoteServer.Services.Database;
using Microsoft.AspNetCore.SignalR;

namespace AetherRemoteServer.SignalR.Handlers;

public class UpdateGlobalPermissionsHandler(DatabaseService database, PresenceService presences, ILogger<UpdateGlobalPermissionsHandler> logger)
{
    public async Task<ActionResponseEc> Handle(string friendCode, UpdateGlobalPermissionsRequest request, IHubCallerClients clients)
    {
        var databaseResultEc = await database.UpdateGlobalPermissions(friendCode, request.Permissions);
        if (databaseResultEc == DatabaseResultEc.Unknown)
            return ActionResponseEc.Unknown;
        
        var permissions = await database.GetAllPermissions(friendCode);
        foreach (var permission in permissions)
        {
            // Ignore pending friends
            if (permission.PermissionsGrantedBy is null)
                continue;
            
            // Only evaluate online friends
            if (presences.TryGet(permission.TargetFriendCode) is not { } target)
                continue;
            
            try
            {
                var resolved = PermissionResolver.Resolve(request.Permissions, permission.PermissionsGrantedTo);
                var command = new SyncPermissionsCommand(friendCode, resolved);
                await clients.Client(target.ConnectionId).SendAsync(HubMethod.SyncPermissions, command);
            }
            catch (Exception e)
            {
                logger.LogError("Syncing online status {Sender} -> {Target} failed, {Error}", friendCode, permission.TargetFriendCode, e);
            }
        }

        return ActionResponseEc.Success;
    }
}