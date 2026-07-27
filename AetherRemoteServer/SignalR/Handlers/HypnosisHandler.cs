using AetherRemoteCommon;
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

public class HypnosisHandler(
    ILogger<HypnosisHandler> logger,
    SessionService sessionService,
    RelayManager relayManager) : IRelayHandler<HypnosisPayload, NoPayload>
{
    private const string Method = HubMethod.Hypnosis;
    private static readonly ResolvedPermissions RequiredPermissions = new(PrimaryPermissions.Hypnosis, SpeakPermissions.None, ElevatedPermissions.None);
    
    public async Task<Response<NoPayload>> Execute(string senderFriendCode, Request<HypnosisPayload> request, IHubCallerClients clients)
    {
        if (ValidateRequest(senderFriendCode, request) is { } error)
        {
            logger.LogWarning("{Sender} sent invalid Hypnosis request {Error}", senderFriendCode, error);
            return new Response<NoPayload>(error, []);
        }
        
        var responses = await relayManager.Relay<HypnosisPayload, NoPayload>(senderFriendCode, Method, request, RequiredPermissions, clients);
        return new Response<NoPayload>(ResponseStatus.Success, responses);
    }
    
    private ResponseStatus? ValidateRequest(string senderFriendCode, Request<HypnosisPayload> request)
    {
        if (sessionService.GetSession(senderFriendCode)?.GeneralBucket.TryConsumeToken() is not true)
            return ResponseStatus.TooManyRequests;
        
        if (VerificationUtilities.ValidFriendCodes(request.TargetFriendCodes) is false)
            return ResponseStatus.BadRequest;
        
        if (request.Payload.Data.SpiralArms is < Constraints.Hypnosis.ArmsMin or > Constraints.Hypnosis.ArmsMax) return ResponseStatus.BadRequest;
        if (request.Payload.Data.SpiralTurns is < Constraints.Hypnosis.TurnsMin or > Constraints.Hypnosis.TurnsMax) return ResponseStatus.BadRequest;
        if (request.Payload.Data.SpiralCurve is < Constraints.Hypnosis.CurvesMin or > Constraints.Hypnosis.CurvesMax) return ResponseStatus.BadRequest;
        if (request.Payload.Data.SpiralThickness is < Constraints.Hypnosis.ThicknessMin or > Constraints.Hypnosis.ThicknessMax) return ResponseStatus.BadRequest;
        if (request.Payload.Data.SpiralSpeed is < Constraints.Hypnosis.SpeedMin or > Constraints.Hypnosis.SpeedMax) return ResponseStatus.BadRequest;
        if (request.Payload.Data.TextDelay is < Constraints.Hypnosis.TextDelayMin or > Constraints.Hypnosis.TextDelayMax) return ResponseStatus.BadRequest;
        if (request.Payload.Data.TextDuration is < Constraints.Hypnosis.TextDurationMin or > Constraints.Hypnosis.TextDurationMax) return ResponseStatus.BadRequest;

        var length = 0;
        foreach (var word in request.Payload.Data.TextWords)
            length += word.Length;
        
        if (length is < Constraints.Hypnosis.TextWordsMin or > Constraints.Hypnosis.TextWordsMax)
            return ResponseStatus.BadRequest;
        
        return null;
    }
}