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

public class CustomizePlusHandler(
    ILogger<CustomizePlusHandler> logger,
    PresenceService presenceService,
    RelayManager relayManager) : IRelayHandler<CustomizePlusPayload, NoPayload>
{
    private const string Method = HubMethod.CustomizePlus;
    private static readonly ResolvedPermissions RequiredPermissions = new(PrimaryPermissions.CustomizePlus, SpeakPermissions.None, ElevatedPermissions.None);
    
    public async Task<Response<NoPayload>> Execute(string senderFriendCode, Request<CustomizePlusPayload> request, IHubCallerClients clients)
    {
        if (ValidateRequest(senderFriendCode, request) is { } error)
        {
            logger.LogWarning("{Sender} sent invalid customize+ request {Error}", senderFriendCode, error);
            return new Response<NoPayload>(error, []);
        }

        var responses = await relayManager.Relay<CustomizePlusPayload, NoPayload>(senderFriendCode, Method, request, RequiredPermissions, clients);
        return new Response<NoPayload>(ResponseStatus.Success, responses);
    }

    private ResponseStatus? ValidateRequest(string senderFriendCode, Request<CustomizePlusPayload> request)
    {
        if (presenceService.IsUserExceedingCooldown(senderFriendCode))
            return ResponseStatus.TooManyRequests;
        
        if (VerificationUtilities.ValidFriendCodes(request.TargetFriendCodes) is false)
            return ResponseStatus.BadRequest;

        if (VerificationUtilities.IsJsonBytes(request.Payload.JsonBoneDataBytes) is false)
            return ResponseStatus.BadRequest;
        
        return null;
    }
}