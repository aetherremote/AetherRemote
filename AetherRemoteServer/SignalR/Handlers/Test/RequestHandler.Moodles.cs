using AetherRemoteCommon.Domain;
using AetherRemoteCommon.Domain.Enums;
using AetherRemoteCommon.Domain.Enums.Permissions;
using AetherRemoteCommon.Domain.Network;
using AetherRemoteCommon.Domain.Network.Moodles;
using AetherRemoteServer.Utilities;
using Microsoft.AspNetCore.SignalR;

namespace AetherRemoteServer.SignalR.Handlers.Test;

public partial class RequestHandler
{
    /// <summary>
    ///     Handles the logic for fulling a <see cref="MoodlesRequest"/>
    /// </summary>
    public async Task<ActionResponse> HandleMoodles(string senderFriendCode, MoodlesRequest request, IHubCallerClients clients)
    {
        if (ValidateEmoteRequest(senderFriendCode, request) is { } error)
        {
            _logger.LogWarning("{Sender} sent invalid moodles request {Error}", senderFriendCode, error);
            return new ActionResponse(error, []);
        }

        var command = new MoodlesCommand(senderFriendCode, request.Info);
        return await _forwardedRequestManager.CheckPermissionsAndSend(
            senderFriendCode, 
            request.TargetFriendCodes, 
            HubMethod.Moodles, 
            new ResolvedPermissions(PrimaryPermissions.Moodles, SpeakPermissions.None, ElevatedPermissions.None), 
            command, 
            clients);
    }
    
    private ActionResponseEc? ValidateEmoteRequest(string senderFriendCode, MoodlesRequest request)
    {
        if (_presenceService.IsUserExceedingCooldown(senderFriendCode))
            return ActionResponseEc.TooManyRequests;
        
        if (VerificationUtilities.ValidFriendCodes(request.TargetFriendCodes) is false)
            return ActionResponseEc.BadDataInRequest;
        
        return null;
    }
}