using AetherRemoteCommon;
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

public class SpeakHandler(
    ILogger<SpeakHandler> logger,
    SessionService sessionService,
    RelayManager relayManager) : IRelayHandler<SpeakPayload, NoPayload>
{
    private const string Method = HubMethod.Speak;
    
    public async Task<Response<NoPayload>> Execute(string senderFriendCode, Request<SpeakPayload> request, IHubCallerClients clients)
    {
        if (ValidateRequest(senderFriendCode, request) is { } error)
        {
            logger.LogWarning("{Sender} sent invalid Speak request {Error}", senderFriendCode, error);
            return new Response<NoPayload>(error, []);
        }

        var permission = request.Payload.ChatChannel.ToSpeakPermissions(request.Payload.Extra);
        if (permission is SpeakPermissions.None)
        {
            logger.LogWarning("{Sender} tried to request with empty permissions {Request}", senderFriendCode, request);
            return new Response<NoPayload>(ResponseStatus.BadRequest, []);
        }

        var required = new ResolvedPermissions(PrimaryPermissions.None, permission, ElevatedPermissions.None);
        var responses = await relayManager.Relay<SpeakPayload, NoPayload>(senderFriendCode, Method, request, required, clients);
        return new Response<NoPayload>(ResponseStatus.Success, responses);
    }
    
    private ResponseStatus? ValidateRequest(string senderFriendCode, Request<SpeakPayload> request)
    {
        if (sessionService.GetSession(senderFriendCode)?.GeneralBucket.TryConsumeToken() is not true)
            return ResponseStatus.TooManyRequests;
        
        if (VerificationUtilities.ValidFriendCodes(request.TargetFriendCodes) is false)
            return ResponseStatus.BadRequest;
        
        if (VerificationUtilities.ValidMessageLengths(request.Payload.Message, request.Payload.Extra) is false)
            return ResponseStatus.BadRequest;

        if (request.TargetFriendCodes.Count > Constraints.MaximumTargetsForInGameOperations)
            return ResponseStatus.TooManyTargets;
            
        return null;
    }
}