using AetherRemoteCommon;
using AetherRemoteCommon.Domain.Enums;
using AetherRemoteCommon.Domain.Network;
using AetherRemoteServer.Managers;
using AetherRemoteServer.Services;
using Microsoft.AspNetCore.SignalR;

namespace AetherRemoteServer.Handlers;

/// <summary>
///     Handles the logic for fulfilling a <see cref="EmoteRequest"/>
/// </summary>
public class EmoteHandler(
    DatabaseService databaseService,
    ConnectedClientsManager connectedClientsManager,
    ILogger<EmoteHandler> logger)
{
    /// <summary>
    ///     Handles the request
    /// </summary>
    public async Task<BaseResponse> Handle(string friendCode, EmoteRequest request, IHubCallerClients clients)
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

        if (request.TargetFriendCodes.Count > Constraints.MaximumTargetsForInGameOperations)
        {
            logger.LogWarning("{Friend} tried to target more than the allowed amount for in-game actions", friendCode);
            return new BaseResponse
            {
                Success = false, 
                Message = "Maximum number of targets exceeded"
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

            if (permissionsGranted.Primary.HasFlag(PrimaryPermissions.Emote) is false)
            {
                logger.LogInformation("{Issuer} targeted {Target} but lacks permissions, skipping", friendCode, target);
                continue;
            }

            try
            {
                var command = new EmoteAction
                {
                    SenderFriendCode = friendCode, 
                    Emote = request.Emote
                };
                
                await clients.Client(connectedClient.ConnectionId).SendAsync(HubMethod.Emote, command);
            }
            catch (Exception e)
            {
                logger.LogWarning("{Issuer} send action to {Target} failed, {Error}", friendCode, target, e.Message);
            }
        }

        return new BaseResponse { Success = true };
    }
}