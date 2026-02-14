using AetherRemoteCommon;
using AetherRemoteCommon.Domain;
using AetherRemoteCommon.Domain.Enums;
using AetherRemoteCommon.Domain.Enums.Permissions;
using AetherRemoteCommon.Domain.Network;
using AetherRemoteCommon.Domain.Network.Speak;
using AetherRemoteCommon.Util;
using AetherRemoteServer.Managers;
using AetherRemoteServer.Services;
using AetherRemoteServer.Utilities;
using Microsoft.AspNetCore.SignalR;

namespace AetherRemoteServer.SignalR.Handlers;

/// <summary>
///     Handles the logic for fulfilling a <see cref="SpeakRequest"/>
/// </summary>
public class SpeakHandler(PresenceService presenceService, ForwardedRequestManager forwardedRequestManager, ILogger<SpeakHandler> logger)
{
    /// <summary>
    ///     Handles the request
    /// </summary>
    public async Task<ActionResponse> Handle(string senderFriendCode, SpeakRequest request, IHubCallerClients clients)
    {
        if (ValidateSpeakRequest(senderFriendCode, request) is { } error)
        {
            logger.LogWarning("{Sender} sent invalid speak request {Error}", senderFriendCode, error);
            return new ActionResponse(error, []);
        }
        
        var speak = request.ChatChannel.ToSpeakPermissions(request.Extra);
        if (speak is SpeakPermissions.None)
            logger.LogWarning("{Sender} tried to request with empty permissions {Request}", senderFriendCode, request);
        
        var permissions = new ResolvedPermissions(PrimaryPermissions.None, speak, ElevatedPermissions.None);
        var command = new SpeakCommand(senderFriendCode, request.Message, request.ChatChannel, request.Extra);
        return await forwardedRequestManager.CheckPermissionsAndSend(senderFriendCode, request.TargetFriendCodes, HubMethod.Speak, permissions, command, clients);
    }

    private ActionResponseEc? ValidateSpeakRequest(string senderFriendCode, SpeakRequest request)
    {
        if (presenceService.IsUserExceedingCooldown(senderFriendCode))
            return ActionResponseEc.TooManyRequests;
        
        if (request.TargetFriendCodes.Count > Constraints.MaximumTargetsForInGameOperations)
            return ActionResponseEc.TooManyTargets;
        
        if (VerificationUtilities.ValidFriendCodes(request.TargetFriendCodes) is false)
            return ActionResponseEc.BadTargets;
        
        if (VerificationUtilities.ValidMessageLengths(request.Message, request.Extra) is false)
            return ActionResponseEc.BadDataInRequest;
        
        return null;
    }
}