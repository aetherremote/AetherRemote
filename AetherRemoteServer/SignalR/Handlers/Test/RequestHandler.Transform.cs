using AetherRemoteCommon.Domain;
using AetherRemoteCommon.Domain.Enums;
using AetherRemoteCommon.Domain.Enums.Permissions;
using AetherRemoteCommon.Domain.Network;
using AetherRemoteCommon.Domain.Network.Transform;
using AetherRemoteCommon.Util;
using AetherRemoteServer.Utilities;
using Microsoft.AspNetCore.SignalR;

namespace AetherRemoteServer.SignalR.Handlers.Test;

public partial class RequestHandler
{
    /// <summary>
    ///     Handles the logic for fulfilling a <see cref="TransformRequest"/>
    /// </summary>
    public async Task<ActionResponse> HandleTransform(string senderFriendCode, TransformRequest request, IHubCallerClients clients)
    {
        if (ValidateEmoteRequest(senderFriendCode, request) is { } error)
        {
            _logger.LogWarning("{Sender} sent invalid transform request {Error}", senderFriendCode, error);
            return new ActionResponse(error, []);
        }

        var primary = request.GlamourerApplyType.ToPrimaryPermission();
        if (primary is PrimaryPermissions.None)
            _logger.LogWarning("{Sender} tried to request with empty permissions {Request}", senderFriendCode, request);

        var elevated = ElevatedPermissions.None;
        if (request.LockCode is not null)
            elevated = ElevatedPermissions.PermanentTransformation;

        var permissions = new ResolvedPermissions(primary, SpeakPermissions.None, elevated);
        var command = new TransformCommand(senderFriendCode, request.GlamourerData, request.GlamourerApplyType, request.LockCode);
        return await _forwardedRequestManager.CheckPermissionsAndSend(
            senderFriendCode, 
            request.TargetFriendCodes, 
            HubMethod.Transform, 
            permissions, 
            command, 
            clients);
    }
    
    private ActionResponseEc? ValidateEmoteRequest(string senderFriendCode, TransformRequest request)
    {
        if (_presenceService.IsUserExceedingCooldown(senderFriendCode))
            return ActionResponseEc.TooManyRequests;
        
        if (VerificationUtilities.ValidFriendCodes(request.TargetFriendCodes) is false)
            return ActionResponseEc.BadDataInRequest;

        return null;
    }
}