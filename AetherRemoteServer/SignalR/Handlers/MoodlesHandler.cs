using AetherRemoteCommon.Domain.Enums;
using AetherRemoteCommon.Domain.Network;
using AetherRemoteServer.Domain.Interfaces;
using Microsoft.AspNetCore.SignalR;

namespace AetherRemoteServer.SignalR.Handlers;

/// <summary>
///     Handles the logic for fulling a <see cref="MoodlesRequest"/>
/// </summary>
public class MoodlesHandler(IClientConnectionService connections, IDatabaseService database, ILogger<MoodlesHandler> logger)
{
    /// <summary>
    ///     Handles the request
    /// </summary>
    public async Task<BaseResponse> Handle(string friendCode, MoodlesRequest request, IHubCallerClients clients)
    {
        if (connections.IsUserExceedingRequestLimit(friendCode))
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
            if (connections.TryGetClient(target) is not { } connectedClient)
            {
                logger.LogInformation("{Issuer} targeted {Target} but they are offline, skipping", friendCode, target);
                continue;
            }
            
            var targetPermissions = await database.GetPermissions(target);
            if (targetPermissions.Permissions.TryGetValue(friendCode, out var permissionsGranted) is false)
            {
                logger.LogInformation("{Issuer} targeted {Target} who is not a friend, skipping", friendCode, target);
                continue;
            }
            
            if (permissionsGranted.Primary.HasFlag(PrimaryPermissions.Moodles) is false)
            {
                logger.LogInformation("{Issuer} targeted {Target} but lacks permissions, skipping", friendCode, target);
                continue;
            }

            try
            {
                var command = new MoodlesAction
                {
                    SenderFriendCode = friendCode,
                    Moodle = request.Moodle
                };
                
                logger.LogInformation("Sending {Moodle} to {FriendCode}", target, request.Moodle);
                
                await clients.Client(connectedClient.ConnectionId).SendAsync(HubMethod.Moodles, command);
            }
            catch (Exception e)
            {
                logger.LogWarning("{Issuer} send action to {Target} failed, {Error}", friendCode, target, e.Message);
            }
        }
        
        return new BaseResponse { Success = true };
    }
}