using AetherRemoteCommon.Domain;
using AetherRemoteCommon.Domain.Enums;
using AetherRemoteCommon.Domain.Enums.Permissions;
using AetherRemoteCommon.Domain.Network;
using AetherRemoteCommon.Domain.Network.Possession.End;
using AetherRemoteCommon.Domain.Network.SyncOnlineStatus;
using AetherRemoteCommon.Util;
using Microsoft.AspNetCore.SignalR;

namespace AetherRemoteServer.SignalR.Handlers.Test;

public partial class RequestHandler
{
    /// <summary>
    ///     Handle the event, removing or adding from the current client list, and updating all the user's friends who are online
    /// </summary>
    public async Task HandleOnlineStatusUpdate(string friendCode, bool online, IHubCallerClients clients)
    {
        if (online is false)
            await HandleOffline(friendCode, clients);
        
        var global = await _databaseService.GetGlobalPermissions(friendCode);
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
                if (online)
                {
                    var resolved = PermissionResolver.Resolve(global, permission.PermissionsGrantedTo);
                    var request = new SyncOnlineStatusCommand(friendCode, FriendOnlineStatus.Online, resolved);
                    await clients.Client(target.ConnectionId).SendAsync(HubMethod.SyncOnlineStatus, request);
                }
                else
                {
                    var request = new SyncOnlineStatusCommand(friendCode, FriendOnlineStatus.Offline, null);
                    await clients.Client(target.ConnectionId).SendAsync(HubMethod.SyncOnlineStatus, request);
                }
            }
            catch (Exception e)
            {
                _logger.LogError("Syncing online status {Sender} -> {Target} failed, {Error}", friendCode, permission.TargetFriendCode, e);
            }
        }
    }

    private async Task HandleOffline(string friendCode, IHubCallerClients clients)
    {
        _presenceService.Remove(friendCode);

        if (_possessionManager.TryGetSession(friendCode) is not { } session)
            return;
        
        _possessionManager.TryRemoveSession(session);
        
        var friendCodeToNotify = session.GhostFriendCode == friendCode ? session.HostFriendCode : session.GhostFriendCode;
        var command = new PossessionEndCommand(friendCode);
        await _forwardedRequestManager.CheckPossessionAndInvoke(
            friendCode, 
            friendCodeToNotify, 
            HubMethod.Possession.End, 
            new ResolvedPermissions(PrimaryPermissions.None, SpeakPermissions.None, ElevatedPermissions.Possession), 
            command, 
            clients);
    }
}