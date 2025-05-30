using AetherRemoteCommon.Domain.Network;
using AetherRemoteServer.Domain.Interfaces;
using Microsoft.AspNetCore.SignalR;

namespace AetherRemoteServer.SignalR.Handlers;

public class UpdateFriendHandler(IClientConnectionService connections, IDatabaseService database, ILogger<UpdateFriendHandler> logger)
{
    public async Task<BaseResponse> Handle(string friendCode, UpdateFriendRequest request, IHubCallerClients clients)
    {
        var success = await database.UpdatePermissions(friendCode, request.TargetFriendCode, request.Permissions);
        
        if (connections.TryGetClient(request.TargetFriendCode) is not { } connectedClient)
            return new BaseResponse { Success = success };

        try
        {
            var sync = new SyncPermissionsAction
            {
                SenderFriendCode = friendCode, 
                PermissionsGrantedBySender = request.Permissions
            };

            await clients.Client(connectedClient.ConnectionId).SendAsync(HubMethod.SyncPermissions, sync);
        }
        catch (Exception e)
        {
            logger.LogWarning("{Issuer} send action to {Target} failed, {Error}", friendCode, request.TargetFriendCode, e.Message);
        }

        return new BaseResponse { Success = success };
    }
}