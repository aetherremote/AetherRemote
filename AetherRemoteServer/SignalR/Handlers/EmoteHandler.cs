using AetherRemoteCommon;
using AetherRemoteCommon.Domain.Enums;
using AetherRemoteCommon.Domain.Network;
using AetherRemoteCommon.V2.Domain;
using AetherRemoteCommon.V2.Domain.Enum;
using AetherRemoteCommon.V2.Domain.Network;
using AetherRemoteCommon.V2.Domain.Network.Base;
using AetherRemoteServer.Domain.Interfaces;
using AetherRemoteServer.SignalR.Handlers.Helpers;
using Microsoft.AspNetCore.SignalR;
using EmoteRequest = AetherRemoteCommon.V2.Domain.Network.EmoteRequest;

namespace AetherRemoteServer.SignalR.Handlers;

/// <summary>
///     Handles the logic for fulfilling a <see cref="AetherRemoteCommon.Domain.Network.EmoteRequest"/>
/// </summary>
public class EmoteHandler(
    TargetAccessResolver targetAccessResolver,
    IClientConnectionService connections,
    ILogger<EmoteHandler> logger)
{
    /// <summary>
    ///     Handles the request
    /// </summary>
    public async Task<ActionResponse> Handle(string senderFriendCode, EmoteRequest request, IHubCallerClients clients)
    {
        if (connections.IsUserExceedingRequestLimit(senderFriendCode))
        {
            logger.LogWarning("{Friend} exceeded request limit", senderFriendCode);
            return new ActionResponse(ActionResponseEc.TooManyRequests);
        }

        if (request.TargetFriendCodes.Count > Constraints.MaximumTargetsForInGameOperations)
        {
            logger.LogWarning("{Friend} tried to target more than the allowed amount for in-game actions", senderFriendCode);
            return new ActionResponse(ActionResponseEc.TooManyTargets);
        }
        
        var results = new Dictionary<string, ActionResultEc>();
        var command = new EmoteForwardedRequest(senderFriendCode, request.Emote, request.DisplayLogMessage);
        var pending = new Task<ActionResult<Unit>>[request.TargetFriendCodes.Count];
        for (var i = 0; i < request.TargetFriendCodes.Count; i++)
        {
            var target = request.TargetFriendCodes[i];
            var connectionIdResult =
                await targetAccessResolver.TryGetAuthorizedConnectionAsync(senderFriendCode, target,
                    PrimaryPermissions.Emote);

            if (connectionIdResult.Result is not ActionResultEc.Success)
            {
                pending[i] = Task.FromResult(ActionResultBuilder.Fail(connectionIdResult.Result));
                continue;
            }

            if (connectionIdResult.Value is not { } connectionId)
            {
                pending[i] = Task.FromResult(ActionResultBuilder.Fail(ActionResultEc.ValueNotSet));
                continue;
            }
            
            try
            {
                var client = clients.Client(connectionId);
                pending[i] = HandlerUtils.AwaitResponsesWithTimeout<Unit>(HubMethod.Emote, client, command);
            }
            catch (Exception e)
            {
                logger.LogWarning("{Issuer} send action to {Target} failed, {Error}", senderFriendCode, target, e.Message);
                pending[i] = Task.FromResult(ActionResultBuilder.Fail(ActionResultEc.Unknown));
            }
        }
        
        var completed = await Task.WhenAll(pending);//.ConfigureAwait(false);
        for(var i = 0; i < completed.Length; i++)
            results.Add(request.TargetFriendCodes[i], completed[i].Result);

        return new ActionResponse(results);
    }
}