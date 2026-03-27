using AetherRemoteCommon.Domain;
using AetherRemoteCommon.Domain.Enums;
using AetherRemoteCommon.Domain.Enums.Permissions;
using AetherRemoteCommon.Domain.Network;
using AetherRemoteCommon.Domain.Network.Honorific;
using AetherRemoteServer.Utilities;
using Microsoft.AspNetCore.SignalR;

namespace AetherRemoteServer.SignalR.Handlers;


public partial class RequestHandler
{
    /// <summary>
    ///     Handles the logic for fulling a <see cref="HonorificRequest"/>
    /// </summary>
    public async Task<ActionResponse> HandleHonorific(string senderFriendCode, HonorificRequest request, IHubCallerClients clients)
    {
        if (ValidateHonorificRequest(senderFriendCode, request) is { } error)
        {
            _logger.LogWarning("{Sender} sent invalid speak request {Error}", senderFriendCode, error);
            return new ActionResponse(error, []);
        }

        var command = new HonorificCommand(senderFriendCode, request.Honorific);
        return await _forwardedRequestManager.CheckPermissionsAndSend(
            senderFriendCode, 
            request.TargetFriendCodes, 
            HubMethod.Honorific, 
            new ResolvedPermissions(PrimaryPermissions.Honorific, SpeakPermissions.None, ElevatedPermissions.None), 
            command, 
            clients);
    }
    
    private ActionResponseEc? ValidateHonorificRequest(string senderFriendCode, HonorificRequest request)
    {
        if (_presenceService.IsUserExceedingCooldown(senderFriendCode))
            return ActionResponseEc.TooManyRequests;
        
        if (VerificationUtilities.ValidFriendCodes(request.TargetFriendCodes) is false)
            return ActionResponseEc.BadDataInRequest;
        
        // TODO: Define rules for validating Honorific data
        
        return null;
    }
}