using AetherRemoteCommon.Domain;
using AetherRemoteCommon.Domain.Enums.Permissions;
using AetherRemoteCommon.Domain.Network;
using AetherRemoteCommon.Network.Domain;
using AetherRemoteCommon.Network.Domain.Payloads;
using AetherRemoteCommon.Network.Enums;
using AetherRemoteServer.Domain.Interfaces;
using AetherRemoteServer.Managers;
using AetherRemoteServer.Services;
using AetherRemoteServer.Utilities;
using Microsoft.AspNetCore.SignalR;

namespace AetherRemoteServer.SignalR.Handlers;

public class MoodlesHandler(
    ILogger<MoodlesHandler> logger,
    SessionService sessionService,
    RelayManager relayManager) : IRelayHandler<MoodlesPayload, NoPayload>
{
    private const string Method = HubMethod.Moodles;
    private static readonly ResolvedPermissions RequiredPermissions = new(PrimaryPermissions.Moodles, SpeakPermissions.None, ElevatedPermissions.None);
    
    public async Task<Response<NoPayload>> Execute(string senderFriendCode, Request<MoodlesPayload> request, IHubCallerClients clients)
    {
        if (ValidateRequest(senderFriendCode, request) is { } error)
        {
            logger.LogWarning("{Sender} sent invalid Moodles request {Error}", senderFriendCode, error);
            return new Response<NoPayload>(error, []);
        }
        
        var responses = await relayManager.Relay<MoodlesPayload, NoPayload>(senderFriendCode, Method, request, RequiredPermissions, clients);
        return new Response<NoPayload>(ResponseStatus.Success, responses);
    }
    
    private ResponseStatus? ValidateRequest(string senderFriendCode, Request<MoodlesPayload> request)
    {
        if (sessionService.GetSession(senderFriendCode)?.GeneralBucket.TryConsumeToken() is not true)
            return ResponseStatus.TooManyRequests;
        
        if (VerificationUtilities.ValidFriendCodes(request.TargetFriendCodes) is false)
            return ResponseStatus.BadRequest;
        
        return null;
    }
}