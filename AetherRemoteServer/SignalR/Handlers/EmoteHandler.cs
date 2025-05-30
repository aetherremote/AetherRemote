using AetherRemoteCommon;
using AetherRemoteCommon.Domain.Enums;
using AetherRemoteCommon.Domain.Network;
using AetherRemoteServer.Domain.Interfaces;
using AetherRemoteServer.SignalR.Handlers.Helpers;
using Microsoft.AspNetCore.SignalR;

namespace AetherRemoteServer.SignalR.Handlers;

/// <summary>
///     Handles the logic for fulfilling a <see cref="EmoteRequest"/>
/// </summary>
public class EmoteHandler(
    TargetAccessResolver targetAccessResolver,
    IClientConnectionService connections,
    ILogger<EmoteHandler> logger)
{
    /// <summary>
    ///     Handles the request
    /// </summary>
    public async Task<BaseResponse> Handle(string senderFriendCode, EmoteRequest request, IHubCallerClients clients)
    {
        if (connections.IsUserExceedingRequestLimit(senderFriendCode))
        {
            logger.LogWarning("{Friend} exceeded request limit", senderFriendCode);
            return new BaseResponse
            {
                Success = false, 
                Message = "Exceeded request limit"
            };
        }

        if (request.TargetFriendCodes.Count > Constraints.MaximumTargetsForInGameOperations)
        {
            logger.LogWarning("{Friend} tried to target more than the allowed amount for in-game actions", senderFriendCode);
            return new BaseResponse
            {
                Success = false, 
                Message = "Maximum number of targets exceeded"
            };
        }

        var command = new EmoteAction(senderFriendCode, request.Emote, request.DisplayLogMessage);
        foreach (var target in request.TargetFriendCodes)
        {
            if (await targetAccessResolver.TryGetAuthorizedConnectionAsync(senderFriendCode, target, PrimaryPermissions.Emote)
                is not { } connectedClient)
                continue;

            if (connectedClient.Value is not { } connectionId)
                continue;
            
            try
            {
                await clients.Client(connectionId).SendAsync(HubMethod.Emote, command);
            }
            catch (Exception e)
            {
                logger.LogWarning("{Issuer} send action to {Target} failed, {Error}", senderFriendCode, target, e.Message);
            }
        }

        return new BaseResponse { Success = true };
    }
}