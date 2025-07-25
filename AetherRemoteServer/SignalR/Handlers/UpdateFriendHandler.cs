using AetherRemoteCommon.Domain.Enums;
using AetherRemoteCommon.Domain.Network;
using AetherRemoteCommon.Domain.Network.SyncPermissions;
using AetherRemoteCommon.Domain.Network.UpdateFriend;
using AetherRemoteServer.Domain.Interfaces;
using Microsoft.AspNetCore.SignalR;

namespace AetherRemoteServer.SignalR.Handlers;

public class UpdateFriendHandler(IConnectionsService connections, IDatabaseService database, ILogger<UpdateFriendHandler> logger)
{
    public async Task<UpdateFriendResponse> Handle(string friendCode, UpdateFriendRequest request, IHubCallerClients clients)
    {
        var databaseResult = await database.UpdatePermissions(friendCode, request.TargetFriendCode, request.Permissions);
        var result = databaseResult switch
        {
            DatabaseResultEc.Success => UpdateFriendEc.Success,
            DatabaseResultEc.NoOp => UpdateFriendEc.NoOp,
            _ => UpdateFriendEc.Unknown
        };
        
        if (connections.TryGetClient(request.TargetFriendCode) is not { } connectedClient)
            return new UpdateFriendResponse(result);
        
        try
        {
            var sync = new SyncPermissionsForwardedRequest(friendCode, request.Permissions);
            await clients.Client(connectedClient.ConnectionId).SendAsync(HubMethod.SyncPermissions, sync);
        }
        catch (Exception e)
        {
            logger.LogWarning("{Issuer} send action to {Target} failed, {Error}", friendCode, request.TargetFriendCode, e.Message);
        }

        return new UpdateFriendResponse(result);
    }
}