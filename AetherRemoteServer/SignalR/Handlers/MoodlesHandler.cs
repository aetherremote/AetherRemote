using AetherRemoteCommon;
using AetherRemoteCommon.Domain;
using AetherRemoteCommon.Domain.Enums;
using AetherRemoteCommon.Domain.Enums.Permissions;
using AetherRemoteCommon.Domain.Network;
using AetherRemoteCommon.Domain.Network.Moodles;
using AetherRemoteServer.Domain.Interfaces;
using AetherRemoteServer.Utilities;
using Microsoft.AspNetCore.SignalR;

namespace AetherRemoteServer.SignalR.Handlers;

/// <summary>
///     Handles the logic for fulling a <see cref="MoodlesRequest"/>
/// </summary>
public class MoodlesHandler(IPresenceService presenceService, IForwardedRequestManager forwardedRequestManager, ILogger<MoodlesHandler> logger)
{
    private const string Method = HubMethod.Moodles;
    private static readonly UserPermissions Permissions = new(PrimaryPermissions2.Moodles, SpeakPermissions2.None, ElevatedPermissions.None);
    
    /// <summary>
    ///     Handles the request
    /// </summary>
    public async Task<ActionResponse> Handle(string senderFriendCode, MoodlesRequest request, IHubCallerClients clients)
    {
        if (ValidateEmoteRequest(senderFriendCode, request) is { } error)
        {
            logger.LogWarning("{Sender} sent invalid moodles request {Error}", senderFriendCode, error);
            return new ActionResponse(error, []);
        }

        var command = new MoodlesCommand(senderFriendCode, request.Info);
        return await forwardedRequestManager.CheckPermissionsAndSend(senderFriendCode, request.TargetFriendCodes, Method, Permissions, command, clients);
    }
    
    private ActionResponseEc? ValidateEmoteRequest(string senderFriendCode, MoodlesRequest request)
    {
        if (presenceService.IsUserExceedingCooldown(senderFriendCode))
            return ActionResponseEc.TooManyRequests;
        
        if (VerificationUtilities.ValidListOfFriendCodes(request.TargetFriendCodes) is false)
            return ActionResponseEc.BadDataInRequest;

        if (request.TargetFriendCodes.Count > Constraints.MaximumTargetsForInGameOperations)
            return ActionResponseEc.TooManyTargets;

        return null;
    }
}