using AetherRemoteCommon.Domain;
using AetherRemoteCommon.Domain.Enums;
using AetherRemoteCommon.Domain.Enums.Permissions;
using AetherRemoteCommon.Domain.Network;
using AetherRemoteCommon.Domain.Network.Moodles;
using AetherRemoteServer.Managers;
using AetherRemoteServer.Services;
using AetherRemoteServer.Utilities;
using Microsoft.AspNetCore.SignalR;

namespace AetherRemoteServer.SignalR.Handlers;

/// <summary>
///     Handles the logic for fulling a <see cref="MoodlesRequest"/>
/// </summary>
public class MoodlesHandler(PresenceService presenceService, ForwardedRequestManager forwardedRequestManager, ILogger<MoodlesHandler> logger)
{
    private const string Method = HubMethod.Moodles;
    private static readonly ResolvedPermissions Permissions = new(PrimaryPermissions.Moodles, SpeakPermissions.None, ElevatedPermissions.None);
    
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
        
        if (VerificationUtilities.ValidFriendCodes(request.TargetFriendCodes) is false)
            return ActionResponseEc.BadDataInRequest;
        
        return null;
    }
}