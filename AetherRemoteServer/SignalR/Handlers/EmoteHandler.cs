using AetherRemoteCommon;
using AetherRemoteCommon.Domain;
using AetherRemoteCommon.Domain.Enums.Permissions;
using AetherRemoteCommon.Domain.Network;
using AetherRemoteCommon.V2;
using AetherRemoteCommon.V2.Domain;
using AetherRemoteCommon.V2.Network.Relays;
using AetherRemoteServer.Managers;
using AetherRemoteServer.Services;
using AetherRemoteServer.SignalR.Handlers.Base;
using AetherRemoteServer.Utilities;
using Microsoft.AspNetCore.SignalR;

namespace AetherRemoteServer.SignalR.Handlers;

public class EmoteHandler(
    ILogger<EmoteHandler> logger,
    PresenceService presenceService,
    RelayManager relayManager) : IRelayHandler<EmotePayload, NoPayload>
{
    private const string Method = HubMethod.Emote;
    private static readonly ResolvedPermissions RequiredPermissions = new(PrimaryPermissions.Emote, SpeakPermissions.None, ElevatedPermissions.None);
    
    public async Task<Response<NoPayload>> Execute(string senderFriendCode, Request<EmotePayload> request, IHubCallerClients clients)
    {
        if (ValidateRequest(senderFriendCode, request) is { } error)
        {
            logger.LogWarning("{Sender} sent invalid speak request {Error}", senderFriendCode, error);
            return new Response<NoPayload>(error, []);
        }
        
        var responses = await relayManager.Relay<EmotePayload, NoPayload>(senderFriendCode, Method, request, RequiredPermissions, clients);
        return new Response<NoPayload>(ResponseStatus.Success, responses);
    }
    
    private ResponseStatus? ValidateRequest(string senderFriendCode, Request<EmotePayload> request)
    {
        if (presenceService.IsUserExceedingCooldown(senderFriendCode))
            return ResponseStatus.TooManyRequests;
        
        if (VerificationUtilities.ValidFriendCodes(request.TargetFriendCodes) is false)
            return ResponseStatus.BadRequest;
        
        if (request.TargetFriendCodes.Count > Constraints.MaximumTargetsForInGameOperations)
            return ResponseStatus.TooManyTargets;
        
        return null;
    }
}