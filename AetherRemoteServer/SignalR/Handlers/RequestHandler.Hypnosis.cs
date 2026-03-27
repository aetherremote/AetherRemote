using AetherRemoteCommon;
using AetherRemoteCommon.Domain;
using AetherRemoteCommon.Domain.Enums;
using AetherRemoteCommon.Domain.Enums.Permissions;
using AetherRemoteCommon.Domain.Network;
using AetherRemoteCommon.Domain.Network.Hypnosis;
using AetherRemoteServer.Utilities;
using Microsoft.AspNetCore.SignalR;

namespace AetherRemoteServer.SignalR.Handlers;

public partial class RequestHandler
{
    /// <summary>
    ///     Handles the logic for fulling a <see cref="HypnosisRequest"/>
    /// </summary>
    public async Task<ActionResponse> HandleHypnosis(string senderFriendCode, HypnosisRequest request, IHubCallerClients clients)
    {
        if (ValidateHypnosisRequest(senderFriendCode, request) is { } error)
        {
            _logger.LogWarning("{Sender} sent invalid hypnosis request {Error}", senderFriendCode, error);
            return new ActionResponse(error, []);
        }

        var command = new HypnosisCommand(senderFriendCode, request.Data);
        return await _forwardedRequestManager.CheckPermissionsAndSend(
            senderFriendCode, 
            request.TargetFriendCodes, 
            HubMethod.Hypnosis, 
            new ResolvedPermissions(PrimaryPermissions.Hypnosis, SpeakPermissions.None, ElevatedPermissions.None), 
            command, 
            clients);
    }

    private ActionResponseEc? ValidateHypnosisRequest(string senderFriendCode, HypnosisRequest request)
    {
        if (_presenceService.IsUserExceedingCooldown(senderFriendCode))
            return ActionResponseEc.TooManyRequests;
        
        if (VerificationUtilities.ValidFriendCodes(request.TargetFriendCodes) is false)
            return ActionResponseEc.BadDataInRequest;
        
        if (request.Data.SpiralArms is < Constraints.Hypnosis.ArmsMin or > Constraints.Hypnosis.ArmsMax) return ActionResponseEc.BadDataInRequest;
        if (request.Data.SpiralTurns is < Constraints.Hypnosis.TurnsMin or > Constraints.Hypnosis.TurnsMax) return ActionResponseEc.BadDataInRequest;
        if (request.Data.SpiralCurve is < Constraints.Hypnosis.CurvesMin or > Constraints.Hypnosis.CurvesMax) return ActionResponseEc.BadDataInRequest;
        if (request.Data.SpiralThickness is < Constraints.Hypnosis.ThicknessMin or > Constraints.Hypnosis.ThicknessMax) return ActionResponseEc.BadDataInRequest;
        if (request.Data.SpiralSpeed is < Constraints.Hypnosis.SpeedMin or > Constraints.Hypnosis.SpeedMax) return ActionResponseEc.BadDataInRequest;
        if (request.Data.TextDelay is < Constraints.Hypnosis.TextDelayMin or > Constraints.Hypnosis.TextDelayMax) return ActionResponseEc.BadDataInRequest;
        if (request.Data.TextDuration is < Constraints.Hypnosis.TextDurationMin or > Constraints.Hypnosis.TextDurationMax) return ActionResponseEc.BadDataInRequest;
        
        var length = 0;
        foreach (var word in request.Data.TextWords)
            length += word.Length;
        
        if (length is < Constraints.Hypnosis.TextWordsMin or > Constraints.Hypnosis.TextWordsMax)
            return ActionResponseEc.BadDataInRequest;

        return null;
    }
    
}