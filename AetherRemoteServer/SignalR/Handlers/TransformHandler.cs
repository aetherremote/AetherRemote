using AetherRemoteCommon.Domain;
using AetherRemoteCommon.Domain.Enums;
using AetherRemoteCommon.Domain.Enums.Permissions;
using AetherRemoteCommon.Domain.Network;
using AetherRemoteCommon.Domain.Network.Transform;
using AetherRemoteCommon.Util;
using AetherRemoteServer.Managers;
using AetherRemoteServer.Services;
using AetherRemoteServer.Utilities;
using Microsoft.AspNetCore.SignalR;

namespace AetherRemoteServer.SignalR.Handlers;

/// <summary>
///     Handles the logic for fulfilling a <see cref="TransformRequest"/>
/// </summary>
public class TransformHandler(PresenceService presenceService, ForwardedRequestManager forwardedRequestManager, ILogger<AddFriendHandler> logger)
{
    private const string Method = HubMethod.Transform;

    /// <summary>
    ///     Handle the request
    /// </summary>
    public async Task<ActionResponse> Handle(string senderFriendCode, TransformRequest request, IHubCallerClients clients)
    {
        if (ValidateEmoteRequest(senderFriendCode, request) is { } error)
        {
            logger.LogWarning("{Sender} sent invalid transform request {Error}", senderFriendCode, error);
            return new ActionResponse(error, []);
        }

        var primary = request.GlamourerApplyType.ToPrimaryPermission();
        if (primary is PrimaryPermissions.None)
            logger.LogWarning("{Sender} tried to request with empty permissions {Request}", senderFriendCode, request);

        var elevated = ElevatedPermissions.None;
        if (request.LockCode is not null)
            elevated = ElevatedPermissions.PermanentTransformation;

        var permissions = new ResolvedPermissions(primary, SpeakPermissions.None, elevated);
        var command = new TransformCommand(senderFriendCode, request.GlamourerData, request.GlamourerApplyType, request.LockCode);
        return await forwardedRequestManager.CheckPermissionsAndSend(senderFriendCode, request.TargetFriendCodes, Method, permissions, command, clients);
    }
    
    private ActionResponseEc? ValidateEmoteRequest(string senderFriendCode, TransformRequest request)
    {
        if (presenceService.IsUserExceedingCooldown(senderFriendCode))
            return ActionResponseEc.TooManyRequests;
        
        if (VerificationUtilities.ValidFriendCodes(request.TargetFriendCodes) is false)
            return ActionResponseEc.BadDataInRequest;

        return null;
    }
}