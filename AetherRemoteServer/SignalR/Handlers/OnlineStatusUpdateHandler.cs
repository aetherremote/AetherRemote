using AetherRemoteCommon.Domain;
using AetherRemoteCommon.Domain.Enums;
using AetherRemoteCommon.Domain.Enums.Permissions;
using AetherRemoteCommon.Domain.Network;
using AetherRemoteCommon.Domain.Network.Possession.End;
using AetherRemoteCommon.Domain.Network.SyncOnlineStatus;
using AetherRemoteServer.Domain.Interfaces;
using Microsoft.AspNetCore.SignalR;

namespace AetherRemoteServer.SignalR.Handlers;

/// <summary>
///     Processes clients connecting and disconnecting from the server
/// </summary>
public class OnlineStatusUpdateHandler(
    IDatabaseService database, 
    IPresenceService presences, 
    IForwardedRequestManager forwarder,
    IPossessionManager possessions,
    ILogger<OnlineStatusUpdateHandler> logger)
{
    private const string Method = HubMethod.Possession.End;
    private static readonly UserPermissions Required = new(PrimaryPermissions2.None, SpeakPermissions2.None, ElevatedPermissions.Possession);
    
    /// <summary>
    ///     Handle the event, removing or adding from the current client list, and updating all the user's friends who are online
    /// </summary>
    public async Task Handle(string friendCode, bool online, IHubCallerClients clients)
    {
        if (online is false)
            await HandleOffline(friendCode, clients);
        
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
                if (online)
                {
                    var request = new SyncOnlineStatusCommand(friendCode, FriendOnlineStatus.Online, permission.PermissionsGrantedTo);
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
                logger.LogError("Syncing online status {Sender} -> {Target} failed, {Error}", friendCode, permission.TargetFriendCode, e);
            }
        }
    }

    private async Task HandleOffline(string friendCode, IHubCallerClients clients)
    {
        presences.Remove(friendCode);

        if (possessions.TryGetSession(friendCode) is not { } session)
            return;
        
        possessions.TryRemoveSession(session);
        
        var friendCodeToNotify = session.GhostFriendCode == friendCode ? session.HostFriendCode : session.GhostFriendCode;
        var command = new PossessionEndCommand(friendCode);
        await forwarder.CheckPossessionAndInvoke(friendCode, friendCodeToNotify, Method, Required, command, clients);
    }
}