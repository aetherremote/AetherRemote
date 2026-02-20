using AetherRemoteCommon.Domain;
using AetherRemoteCommon.Domain.Enums;
using AetherRemoteCommon.Domain.Enums.Permissions;
using AetherRemoteCommon.Domain.Network;
using AetherRemoteCommon.Domain.Network.Customize;
using AetherRemoteServer.Utilities;
using Microsoft.AspNetCore.SignalR;

namespace AetherRemoteServer.SignalR.Handlers.Test;

public partial class RequestHandler
{
    /// <summary>
    ///     Handles the logic for fulfilling a <see cref="CustomizeRequest"/>
    /// </summary>
    public async Task<ActionResponse> HandleCustomizePlus(string senderFriendCode, CustomizeRequest request, IHubCallerClients clients)
    {
        if (ValidateCustomizeRequest(senderFriendCode, request) is { } error)
        {
            _logger.LogWarning("{Sender} sent invalid customize+ request {Error}", senderFriendCode, error);
            return new ActionResponse(error, []);
        }
        
        var forwardedRequest = new CustomizeCommand(senderFriendCode, request.JsonBoneDataBytes, request.Additive);
        return await _forwardedRequestManager.CheckPermissionsAndSend(
            senderFriendCode, 
            request.TargetFriendCodes, 
            HubMethod.CustomizePlus, 
            new ResolvedPermissions(PrimaryPermissions.CustomizePlus, SpeakPermissions.None, ElevatedPermissions.None), 
            forwardedRequest, 
            clients);
    }
    
    private ActionResponseEc? ValidateCustomizeRequest(string senderFriendCode, CustomizeRequest request)
    {
        if (_presenceService.IsUserExceedingCooldown(senderFriendCode))
            return ActionResponseEc.TooManyRequests;
        
        if (VerificationUtilities.ValidFriendCodes(request.TargetFriendCodes) is false)
            return ActionResponseEc.BadDataInRequest;

        if (VerificationUtilities.IsJsonBytes(request.JsonBoneDataBytes) is false)
            return ActionResponseEc.BadDataInRequest;
        
        return null;
    }
}