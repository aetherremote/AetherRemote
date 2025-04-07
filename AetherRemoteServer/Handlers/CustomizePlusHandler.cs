using AetherRemoteCommon.Domain.Enums;
using AetherRemoteCommon.Domain.Network;
using AetherRemoteServer.Managers;
using AetherRemoteServer.Services;
using Microsoft.AspNetCore.SignalR;

namespace AetherRemoteServer.Handlers;

public class CustomizePlusHandler(
    DatabaseService databaseService,
    ConnectedClientsManager connectedClientsManager,
    ILogger<CustomizePlusHandler> logger)
{
    /// <summary>
    ///     Handles the request
    /// </summary>
    public async Task<BaseResponse> Handle(string friendCode, CustomizePlusRequest request, IHubCallerClients clients)
    {
        if (connectedClientsManager.IsUserExceedingRequestLimit(friendCode))
        {
            logger.LogWarning("{Friend} exceeded request limit", friendCode);
            return new BaseResponse
            {
                Success = false, 
                Message = "Exceeded request limit"
            };
        }

        foreach (var target in request.TargetFriendCodes)
        {
            if (connectedClientsManager.ConnectedClients.TryGetValue(target, out var connectedClient) is false)
            {
                logger.LogInformation("{Issuer} targeted {Target} but they are offline, skipping", friendCode, target);
                continue;
            }
            
            var targetPermissions = await databaseService.GetPermissions(target);
            if (targetPermissions.Permissions.TryGetValue(friendCode, out var permissionsGranted) is false)
            {
                logger.LogInformation("{Issuer} targeted {Target} who is not a friend, skipping", friendCode, target);
                continue;
            }
            
            if (permissionsGranted.Primary.HasFlag(PrimaryPermissions.CustomizePlus) is false)
            {
                logger.LogInformation("{Issuer} targeted {Target} but lacks permissions, skipping", friendCode, target);
                continue;
            }

            try
            {
                var command = new CustomizePlusAction
                {
                    SenderFriendCode = friendCode,
                    Customize = request.Customize
                };
                
                logger.LogInformation("Sending {Customize} to {FriendCode}", request.Customize, target);
                
                await clients.Client(connectedClient.ConnectionId).SendAsync(HubMethod.CustomizePlus, command);
            }
            catch (Exception e)
            {
                logger.LogWarning("{Issuer} send action to {Target} failed, {Error}", friendCode, target, e.Message);
            }
        }
        
        return new BaseResponse { Success = true };
    }
}