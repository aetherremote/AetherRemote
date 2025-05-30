using AetherRemoteCommon;
using AetherRemoteCommon.Domain.Enums;
using AetherRemoteCommon.Domain.Network;
using AetherRemoteCommon.Util;
using AetherRemoteServer.Domain.Interfaces;
using Microsoft.AspNetCore.SignalR;

namespace AetherRemoteServer.SignalR.Handlers;

/// <summary>
///     Handles the logic for fulfilling a <see cref="SpeakRequest"/>
/// </summary>
public class SpeakHandler(IClientConnectionService connections, IDatabaseService database, ILogger<SpeakHandler> logger)
{
    /// <summary>
    ///     Handles the request
    /// </summary>
    public async Task<BaseResponse> Handle(string friendCode, SpeakRequest request, IHubCallerClients clients)
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

            if (request.ChatChannel is ChatChannel.Linkshell or ChatChannel.CrossWorldLinkshell)
            {
                if (int.TryParse(request.Extra, out var linkshell) is false)
                {
                    logger.LogInformation("{Issuer} requested an invalid linkshell number, aborting", friendCode);
                    return new BaseResponse
                    {
                        Success = false,
                        Message = "Invalid linkshell number"
                    };
                }

                if (PermissionsChecker.Speak(permissionsGranted.Linkshell, linkshell) is false)
                {
                    logger.LogInformation("{Issuer} targeted {Target} but lacks permissions, skipping", friendCode, target);
                    continue;
                }
            }
            else
            {
                if (PermissionsChecker.Speak(permissionsGranted.Primary, request.ChatChannel) is false)
                {
                    logger.LogInformation("{Issuer} targeted {Target} but lacks permissions, skipping", friendCode, target);
                    continue;
                }
            }
            
            try
            {
                var command = new SpeakAction
                {
                    SenderFriendCode = friendCode, 
                    Message = request.Message, 
                    ChatChannel = request.ChatChannel,
                    Extra = request.Extra
                };
                
                await clients.Client(connectedClient.ConnectionId).SendAsync(HubMethod.Speak, command);
            }
            catch (Exception e)
            {
                logger.LogWarning("{Issuer} send action to {Target} failed, {Error}", friendCode, target, e.Message);
            }
        }
        
        return new BaseResponse { Success = true };
    }
}