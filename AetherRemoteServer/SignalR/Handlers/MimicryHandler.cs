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

public class MimicryHandler(
    ILogger<MoodlesHandler> logger,
    SessionService sessionService,
    RelayManager relayManager) : IRelayHandler<MimicryPayload, MimicryResponse>
{
    private const string Method = HubMethod.Mimicry;
    private static readonly ResolvedPermissions RequiredPermissions = new(PrimaryPermissions.Mimicry, SpeakPermissions.None, ElevatedPermissions.None);
    
    public async Task<Response<MimicryResponse>> Execute(string senderFriendCode, Request<MimicryPayload> request, IHubCallerClients clients)
    {
        if (ValidateRequest(senderFriendCode, request) is { } error)
        {
            logger.LogWarning("{Sender} sent invalid mimicry request {Error}", senderFriendCode, error);
            return new Response<MimicryResponse>(error, []);
        }
        
        var responses = await relayManager.Relay<MimicryPayload, NoPayload>(senderFriendCode, Method, request, RequiredPermissions, clients);

        // This is just for good practice, we know both in the list and the dictionary these values are set
        var targetFriendCode = request.TargetFriendCodes.First();
        if (responses.TryGetValue(targetFriendCode, out var mimicryResponse) is false)
        {
            logger.LogWarning("{TargetFriendCode} mimicry target not found in responses", targetFriendCode);
            return new Response<MimicryResponse>(ResponseStatus.BadRequest, []);
        }
        
        // Return early here since the request succeeded but the client's response didn't
        if (mimicryResponse is not RoutedResponseStatus.Success)
            return new Response<MimicryResponse>(ResponseStatus.Success, responses);

        if (sessionService.GetSession(targetFriendCode) is not { } session)
        {
            logger.LogWarning("{TargetFriendCode} went offline in the middle of a request", targetFriendCode);
            return new Response<MimicryResponse>(ResponseStatus.TargetOffline, responses);
        }
        
        var payload = new MimicryResponse(session.CharacterName, session.CharacterWorld);
        return new Response<MimicryResponse>(ResponseStatus.Success, responses, payload);
    }
    
    private ResponseStatus? ValidateRequest(string senderFriendCode, Request<MimicryPayload> request)
    {
        if (sessionService.GetSession(senderFriendCode)?.GeneralBucket.TryConsumeToken() is not true)
            return ResponseStatus.TooManyRequests;
        
        if (VerificationUtilities.ValidFriendCodes(request.TargetFriendCodes) is false)
            return ResponseStatus.BadRequest;

        return request.TargetFriendCodes.Count switch
        {
            > 1 => ResponseStatus.TooManyTargets,
            < 1 => ResponseStatus.TooFewTargets,
            _ => null
        };
    }
}