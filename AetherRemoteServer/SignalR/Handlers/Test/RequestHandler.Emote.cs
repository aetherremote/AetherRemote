using AetherRemoteCommon;
using AetherRemoteCommon.Domain;
using AetherRemoteCommon.Domain.Enums;
using AetherRemoteCommon.Domain.Enums.Permissions;
using AetherRemoteCommon.Domain.Network;
using AetherRemoteCommon.Domain.Network.Emote;
using AetherRemoteServer.Utilities;
using Microsoft.AspNetCore.SignalR;

namespace AetherRemoteServer.SignalR.Handlers.Test;

public partial class RequestHandler
{
    /// <summary>
    ///     Handles the logic for fulfilling a <see cref="EmoteRequest"/>
    /// </summary>
    public async Task<ActionResponse> HandleEmote(string senderFriendCode, EmoteRequest request, IHubCallerClients clients)
    {
        if (ValidateEmoteRequest(senderFriendCode, request) is { } error)
        {
            _logger.LogWarning("{Sender} sent invalid speak request {Error}", senderFriendCode, error);
            return new ActionResponse(error, []);
        }
        
        var command = new EmoteCommand(senderFriendCode, request.Emote, request.DisplayLogMessage);
        return await _forwardedRequestManager.CheckPermissionsAndSend(
            senderFriendCode, 
            request.TargetFriendCodes, 
            HubMethod.Emote, 
            new ResolvedPermissions(PrimaryPermissions.Emote, SpeakPermissions.None, ElevatedPermissions.None), 
            command, 
            clients);
    }

    private ActionResponseEc? ValidateEmoteRequest(string senderFriendCode, EmoteRequest request)
    {
        if (_presenceService.IsUserExceedingCooldown(senderFriendCode))
            return ActionResponseEc.TooManyRequests;
        
        if (VerificationUtilities.ValidFriendCodes(request.TargetFriendCodes) is false)
            return ActionResponseEc.BadDataInRequest;

        if (request.TargetFriendCodes.Count > Constraints.MaximumTargetsForInGameOperations)
            return ActionResponseEc.TooManyTargets;

        return null;
    }
}