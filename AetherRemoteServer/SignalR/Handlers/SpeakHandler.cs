using AetherRemoteCommon;
using AetherRemoteCommon.Domain;
using AetherRemoteCommon.Domain.Enums;
using AetherRemoteCommon.Domain.Enums.Permissions;
using AetherRemoteCommon.Domain.Network;
using AetherRemoteCommon.Domain.Network.Speak;
using AetherRemoteCommon.Util;
using AetherRemoteServer.Domain.Interfaces;
using AetherRemoteServer.Utilities;
using Microsoft.AspNetCore.SignalR;

namespace AetherRemoteServer.SignalR.Handlers;

/// <summary>
///     Handles the logic for fulfilling a <see cref="SpeakRequest"/>
/// </summary>
public class SpeakHandler(IPresenceService presenceService, IForwardedRequestManager forwardedRequestManager, ILogger<SpeakHandler> logger)
{
    /// <summary>
    ///     Handles the request
    /// </summary>
    public async Task<ActionResponse> Handle(string senderFriendCode, SpeakRequest request, IHubCallerClients clients)
    {
        if (ValidateSpeakRequest(senderFriendCode, request) is { } error)
        {
            logger.LogWarning("{Sender} sent invalid speak request", senderFriendCode);
            return new ActionResponse(error, []);
        }
        
        var speak = request.ChatChannel.ToSpeakPermissions(request.Extra);
        if (speak is SpeakPermissions2.None)
            logger.LogWarning("{Sender} tried to request with empty permissions {Request}", senderFriendCode, request);
        
        var permissions = new UserPermissions(PrimaryPermissions2.None, speak, ElevatedPermissions.None);
        var command = new SpeakCommand(senderFriendCode, request.Message, request.ChatChannel, request.Extra);
        return await forwardedRequestManager.CheckPermissionsAndSend(senderFriendCode, request.TargetFriendCodes, HubMethod.Speak, permissions, command, clients);
    }

    private ActionResponseEc? ValidateSpeakRequest(string senderFriendCode, SpeakRequest request)
    {
        if (presenceService.IsUserExceedingCooldown(senderFriendCode))
            return ActionResponseEc.UnexpectedState;
        
        if (request.TargetFriendCodes.Count > Constraints.MaximumTargetsForInGameOperations)
            return ActionResponseEc.TooManyTargets;
        
        if (VerificationUtilities.ValidListOfFriendCodes(request.TargetFriendCodes) is false)
            return ActionResponseEc.BadTargets;
        
        if (VerificationUtilities.ValidMessageLengths(request.Message, request.Extra) is false)
            return ActionResponseEc.BadDataInRequest;
        
        return null;
    }
}