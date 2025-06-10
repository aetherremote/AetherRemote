using AetherRemoteCommon.Domain;
using AetherRemoteCommon.Domain.Network;
using AetherRemoteCommon.V2.Domain.Enum;
using AetherRemoteCommon.V2.Domain.Network.AddFriend;
using AetherRemoteCommon.V2.Domain.Network.SyncOnlineStatus;
using AetherRemoteServer.Domain.Interfaces;
using Microsoft.AspNetCore.SignalR;

namespace AetherRemoteServer.SignalR.Handlers;

/// <summary>
///     Handles the logic for fulfilling a <see cref="AddFriendRequest"/>
/// </summary>
public class AddFriendHandler(
    IConnectionsService connections, 
    IDatabaseService database,
    ILogger<AddFriendHandler> logger)
{
    /// <summary>
    ///     Handles the request
    /// </summary>
    public async Task<AddFriendResponse> Handle(string friendCode, AddFriendRequest request, IHubCallerClients clients)
    {
        var result = await database.CreatePermissions(friendCode, request.TargetFriendCode);
        var code = result switch
        {
            DatabaseResultEc.Uninitialized => AddFriendEc.Uninitialized,
            DatabaseResultEc.NoSuchFriendCode => AddFriendEc.NoSuchFriendCode,
            DatabaseResultEc.AlreadyFriends => AddFriendEc.AlreadyFriends,
            DatabaseResultEc.Success => AddFriendEc.Success,
            _ => AddFriendEc.Unknown
        };

        // Check if they are online
        var target = connections.TryGetClient(request.TargetFriendCode);
        if (target is null)
            return new AddFriendResponse(code, false);
        
        // Check if they are friends with the person who added them
        var permissions = await database.GetPermissions(request.TargetFriendCode);
        if (permissions.Permissions.ContainsKey(friendCode) is false)
            return new AddFriendResponse(code, false);
        
        try
        {
            var sync = new SyncOnlineStatusForwardedRequest(friendCode, true, new UserPermissions());
            await clients.Client(target.ConnectionId).SendAsync(HubMethod.SyncOnlineStatus, sync);
        }
        catch (Exception e)
        {
            logger.LogWarning("Unable to sync {FriendCode}'s online status to {Target} while adding them, {Exception}", 
                friendCode, target, e.Message);
        }
        
        return new AddFriendResponse(code, true);
    }
}