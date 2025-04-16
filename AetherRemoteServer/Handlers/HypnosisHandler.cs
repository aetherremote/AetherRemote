using AetherRemoteCommon.Domain;
using AetherRemoteCommon.Domain.Enums;
using AetherRemoteCommon.Domain.Network;
using AetherRemoteServer.Managers;
using AetherRemoteServer.Services;
using Microsoft.AspNetCore.SignalR;

namespace AetherRemoteServer.Handlers;

public class HypnosisHandler(
    DatabaseService databaseService,
    ConnectedClientsManager connectedClientsManager,
    ILogger<HypnosisHandler> logger)
{
    public async Task<BaseResponse> Handle(string friendCode, HypnosisRequest request, IHubCallerClients clients)
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
            
            if (permissionsGranted.Primary.HasFlag(PrimaryPermissions.Hypnosis) is false)
            {
                logger.LogInformation("{Issuer} targeted {Target} but lacks permissions, skipping", friendCode, target);
                continue;
            }

            try
            {
                var command = new HypnosisAction
                {
                    SenderFriendCode = friendCode,
                    Spiral = new SpiralInfo
                    {
                        Duration = request.Spiral.Duration,
                        Speed = request.Spiral.Speed,
                        TextSpeed = request.Spiral.TextSpeed,
                        Color = request.Spiral.Color,
                        TextColor = request.Spiral.TextColor,
                        TextMode = request.Spiral.TextMode,
                        WordBank = request.Spiral.WordBank
                    }
                };
                
                logger.LogInformation("Sending {Spiral} to {FriendCode}", command, target);
                
                await clients.Client(connectedClient.ConnectionId).SendAsync(HubMethod.Hypnosis, command);
            }
            catch (Exception e)
            {
                logger.LogWarning("{Issuer} send action to {Target} failed, {Error}", friendCode, target, e.Message);
            }
        }
        
        return new BaseResponse { Success = true };
    }
}