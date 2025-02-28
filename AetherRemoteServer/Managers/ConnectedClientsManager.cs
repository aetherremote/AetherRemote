using System.Collections.Concurrent;
using AetherRemoteCommon;
using AetherRemoteCommon.Domain.Network;
using AetherRemoteServer.Domain;
using AetherRemoteServer.Services;
using Microsoft.AspNetCore.SignalR;

namespace AetherRemoteServer.Managers;

public class ConnectedClientsManager(DatabaseService databaseService, ILogger<ConnectedClientsManager> logger)
{
    /// <summary>
    ///     Maps FriendCode to <see cref="ConnectedClientInfo"/>
    /// </summary>
    public readonly ConcurrentDictionary<string, ConnectedClientInfo> ConnectedClients = [];

    /// <summary>
    ///     Checks to see if provided friend code is sending messages too quickly
    /// </summary>
    public bool IsUserExceedingRequestLimit(string issuerFriendCode)
    {
        if (ConnectedClients.TryGetValue(issuerFriendCode, out var issuer))
            return (DateTime.UtcNow - issuer.LastAction).TotalSeconds < Constraints.ExternalCommandCooldownInSeconds;

        logger.LogWarning("A de-sync may have occurred, {Friend} is not in connected client list", issuerFriendCode);
        return true;
    }
    
    /// <summary>
    ///     Handles whenever a client connects or disconnects
    /// </summary>
    public async Task ProcessFriendOnlineStatusChange(string issuerFriendCode, bool online, HubCallerContext context,
        IHubCallerClients clients)
    {
        if (online)
        {
            if (ConnectedClients.TryAdd(issuerFriendCode, new ConnectedClientInfo(context.ConnectionId)) is false)
                logger.LogWarning("A de-sync may have occurred, {Friend} is already online", issuerFriendCode);
        }
        else
        {
            if (ConnectedClients.TryRemove(issuerFriendCode, out _) is false)
                logger.LogWarning("A de-sync may have occurred, {Friend} is already offline", issuerFriendCode);
        }

        var friendPermissions = await databaseService.GetPermissions(issuerFriendCode);
        foreach (var friend in friendPermissions.Permissions)
        {
            if (ConnectedClients.TryGetValue(friend.Key, out var connectedFriend) is false)
                continue;

            try
            {
                var request = new SyncOnlineStatusAction
                {
                    SenderFriendCode = issuerFriendCode, 
                    Online = online,
                    Permissions = online ? friend.Value : null
                };
                
                await clients.Client(connectedFriend.ConnectionId).SendAsync(HubMethod.SyncOnlineStatus, request);
            }
            catch (Exception e)
            {
                logger.LogWarning("Unable to sync {FriendCode}'s online status to {Target}, {Exception}", issuerFriendCode,
                    friend.Key, e.Message);
            }
        }
    }
}