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

public class TwinningHandler(
    ILogger<TwinningHandler> logger,
    PresenceService presenceService,
    RelayManager relayManager) : IRelayHandler<TwinningPayload, NoPayload>
{
    private const string Method = HubMethod.Twinning;
    
    public async Task<Response<NoPayload>> Execute(string senderFriendCode, Request<TwinningPayload> request, IHubCallerClients clients)
    {
        if (ValidateRequest(senderFriendCode, request) is { } error)
        {
            logger.LogWarning("{Sender} sent invalid Transformation request {Error}", senderFriendCode, error);
            return new Response<NoPayload>(error, []);
        }
        
        // TODO: Does this need None permission checking...?
        var primaryPermissions = request.Payload.SwapAttributes.ToPrimaryPermissions();
        primaryPermissions |= PrimaryPermissions.Twinning;
        
        var elevatedPermissions = ElevatedPermissions.None;
        if (request.Payload.LockCode is not null)
            elevatedPermissions = ElevatedPermissions.PermanentTransformation;
        
        var required = new ResolvedPermissions(primaryPermissions, SpeakPermissions.None, elevatedPermissions);
        var responses = await relayManager.Relay<TwinningPayload, NoPayload>(senderFriendCode, Method, request, required, clients);
        return new Response<NoPayload>(ResponseStatus.Success, responses);
    }
    
    private ResponseStatus? ValidateRequest(string senderFriendCode, Request<TwinningPayload> request)
    {
        if (presenceService.IsUserExceedingCooldown(senderFriendCode))
            return ResponseStatus.TooManyRequests;
        
        if (VerificationUtilities.ValidFriendCodes(request.TargetFriendCodes) is false)
            return ResponseStatus.BadRequest;
        
        // TODO: Transformation Specific Validation
        
        return null;
    }
}