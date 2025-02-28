using AetherRemoteCommon.Domain.Network;
using AetherRemoteServer.Managers;
using AetherRemoteServer.Services;
using Microsoft.AspNetCore.SignalR;

namespace AetherRemoteServer.Handlers;

public class UpdateFriendHandler(
    ConnectedClientsManager connectedClientsManager,
    DatabaseService databaseService,
    ILogger<UpdateFriendHandler> logger)
{
    public async Task<BaseResponse> Handle(string friendCode, UpdateFriendRequest request, IHubCallerClients clients)
    {
        var success = await databaseService.UpdatePermissions(friendCode, request.TargetFriendCode, request.Permissions);
        if (connectedClientsManager.ConnectedClients.TryGetValue(request.TargetFriendCode, out var connectedClient) is false)
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