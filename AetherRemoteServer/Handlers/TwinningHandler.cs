using AetherRemoteCommon.Domain.Enums;
using AetherRemoteCommon.Domain.Network;
using AetherRemoteServer.Managers;
using AetherRemoteServer.Services;
using Microsoft.AspNetCore.SignalR;

namespace AetherRemoteServer.Handlers;

/// <summary>
///     Handles the logic for fulfilling a <see cref="TwinningRequest"/>
/// </summary>
public class TwinningHandler(
    DatabaseService databaseService,
    ConnectedClientsManager connectedClientsManager,
    ILogger<AddFriendHandler> logger)
{
    /// <summary>
    ///     Handle the request
    /// </summary>
    public async Task<BaseResponse> Handle(string friendCode, TwinningRequest request, IHubCallerClients clients)
    {
        if (connectedClientsManager.IsUserExceedingRequestLimit(friendCode))
        {
            logger.LogWarning("{Friend} exceeded request limit", friendCode);
            return new BaseResponse { Success = false, Message = "Exceeded request limit" };
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

            if (permissionsGranted.Primary.HasFlag(PrimaryPermissions.Twinning) is false)
            {
                logger.LogInformation("{Issuer} targeted {Target} but lacks permissions, skipping", friendCode, target);
                continue;
            }

            if (request.SwapMods && permissionsGranted.Primary.HasFlag(PrimaryPermissions.Mods) is false)
            {
                logger.LogInformation("{Issuer} targeted {Target} but lacks mod permissions, skipping", friendCode, target);
                continue;
            }

            try
            {
                var command = new TwinningAction
                {
                    SenderFriendCode = friendCode,
                    Identity = request.Identity,
                    SwapMods = request.SwapMods
                };
                
                await clients.Client(connectedClient.ConnectionId).SendAsync(HubMethod.Twinning, command);
            }
            catch (Exception e)
            {
                logger.LogWarning("{Issuer} send action to {Target} failed, {Error}", friendCode, target, e.Message);
            }
        }

        return new BaseResponse { Success = true };
    }
}