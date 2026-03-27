using AetherRemoteCommon.Domain;
using AetherRemoteCommon.Domain.Enums;
using AetherRemoteCommon.Domain.Enums.Permissions;
using AetherRemoteCommon.Domain.Network;
using AetherRemoteCommon.Domain.Network.HypnosisStop;
using AetherRemoteServer.Utilities;
using Microsoft.AspNetCore.SignalR;

namespace AetherRemoteServer.SignalR.Handlers;

public partial class RequestHandler
{
    /// <summary>
    ///     Handles the logic for fulling a <see cref="HypnosisStopRequest"/>
    /// </summary>
    public async Task<ActionResponse> HandleHypnosisStop(string senderFriendCode, HypnosisStopRequest request, IHubCallerClients clients)
    {
        if (ValidateEmoteRequest(senderFriendCode, request) is { } error)
        {
            _logger.LogWarning("{Sender} sent invalid hypnosis stop request {Error}", senderFriendCode, error);
            return new ActionResponse(error, []);
        }
        
        var command = new HypnosisStopCommand(senderFriendCode);
        return await _forwardedRequestManager.CheckPermissionsAndSend(
            senderFriendCode, 
            request.TargetFriendCodes, 
            HubMethod.HypnosisStop, 
            new ResolvedPermissions(PrimaryPermissions.Hypnosis, SpeakPermissions.None, ElevatedPermissions.None), 
            command, 
            clients);
    }
    
    private ActionResponseEc? ValidateEmoteRequest(string senderFriendCode, HypnosisStopRequest request)
    {
        if (_presenceService.IsUserExceedingCooldown(senderFriendCode))
            return ActionResponseEc.TooManyRequests;
        
        if (VerificationUtilities.ValidFriendCodes(request.TargetFriendCodes) is false)
            return ActionResponseEc.BadDataInRequest;

        return null;
    }
}