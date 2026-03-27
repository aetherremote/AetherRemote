using AetherRemoteCommon.Domain.Enums;
using AetherRemoteCommon.Domain.Network;
using AetherRemoteCommon.Domain.Network.SyncPermissions;
using AetherRemoteCommon.Domain.Network.UpdateGlobalPermissions;
using AetherRemoteCommon.Util;
using Microsoft.AspNetCore.SignalR;

namespace AetherRemoteServer.SignalR.Handlers;

public partial class RequestHandler
{
    public async Task<ActionResponseEc> HandleUpdateGlobalPermissions(string friendCode, UpdateGlobalPermissionsRequest request, IHubCallerClients clients)
    {
        var databaseResultEc = await _databaseService.UpdateGlobalPermissions(friendCode, request.Permissions);
        if (databaseResultEc == DatabaseResultEc.Unknown)
            return ActionResponseEc.Unknown;
        
        var permissions = await _databaseService.GetAllPermissions(friendCode);
        foreach (var permission in permissions)
        {
            // Ignore pending friends
            if (permission.PermissionsGrantedBy is null)
                continue;
            
            // Only evaluate online friends
            if (_presenceService.TryGet(permission.TargetFriendCode) is not { } target)
                continue;
            
            try
            {
                var resolved = PermissionResolver.Resolve(request.Permissions, permission.PermissionsGrantedTo);
                var command = new SyncPermissionsCommand(friendCode, resolved);
                await clients.Client(target.ConnectionId).SendAsync(HubMethod.SyncPermissions, command);
            }
            catch (Exception e)
            {
                _logger.LogError("Syncing online status {Sender} -> {Target} failed, {Error}", friendCode, permission.TargetFriendCode, e);
            }
        }

        return ActionResponseEc.Success;
    }
}