using AetherRemoteCommon.Domain;
using AetherRemoteCommon.Domain.Enums.Permissions;
using AetherRemoteCommon.Domain.Network;
using AetherRemoteCommon.Network.Domain;
using AetherRemoteCommon.Network.Domain.Payloads;
using AetherRemoteCommon.Network.Enums;
using AetherRemoteCommon.Util;
using AetherRemoteServer.Domain.Interfaces;
using AetherRemoteServer.Managers;
using AetherRemoteServer.Services;
using AetherRemoteServer.Utilities;
using Microsoft.AspNetCore.SignalR;

namespace AetherRemoteServer.SignalR.Handlers;

public class TransformationHandler(
    ILogger<TransformationHandler> logger,
    SessionService sessionService,
    RelayManager relayManager) : IRelayHandler<TransformationPayload, NoPayload>
{
    private const string Method = HubMethod.Transform;
    
    public async Task<Response<NoPayload>> Execute(string senderFriendCode, Request<TransformationPayload> request, IHubCallerClients clients)
    {
        if (ValidateRequest(senderFriendCode, request) is { } error)
        {
            logger.LogWarning("{Sender} sent invalid Transformation request {Error}", senderFriendCode, error);
            return new Response<NoPayload>(error, []);
        }

        // TODO: Does this need the Transform permission appended...?
        var primaryPermissions = request.Payload.GlamourerApplyType.ToPrimaryPermission();
        if (primaryPermissions is PrimaryPermissions.None)
        {
            logger.LogWarning("{Sender} tried to request with empty permissions {Request}", senderFriendCode, request);
            return new Response<NoPayload>(ResponseStatus.BadRequest, []);
        }
        
        var elevatedPermissions = ElevatedPermissions.None;
        if (request.Payload.LockCode is not null)
            elevatedPermissions = ElevatedPermissions.PermanentTransformation;
        
        var required = new ResolvedPermissions(primaryPermissions, SpeakPermissions.None, elevatedPermissions);
        var responses = await relayManager.Relay<TransformationPayload, NoPayload>(senderFriendCode, Method, request, required, clients);
        return new Response<NoPayload>(ResponseStatus.Success, responses);
    }
    
    private ResponseStatus? ValidateRequest(string senderFriendCode, Request<TransformationPayload> request)
    {
        if (sessionService.GetSession(senderFriendCode)?.GeneralBucket.TryConsumeToken() is not true)
            return ResponseStatus.TooManyRequests;
        
        if (VerificationUtilities.ValidFriendCodes(request.TargetFriendCodes) is false)
            return ResponseStatus.BadRequest;
        
        // TODO: Transformation Specific Validation
        
        return null;
    }
}