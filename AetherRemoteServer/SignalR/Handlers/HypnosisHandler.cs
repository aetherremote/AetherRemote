using AetherRemoteCommon;
using AetherRemoteCommon.Domain;
using AetherRemoteCommon.Domain.Enums;
using AetherRemoteCommon.Domain.Enums.Permissions;
using AetherRemoteCommon.Domain.Network;
using AetherRemoteCommon.Domain.Network.Hypnosis;
using AetherRemoteServer.Domain.Interfaces;
using AetherRemoteServer.Utilities;
using Microsoft.AspNetCore.SignalR;

namespace AetherRemoteServer.SignalR.Handlers;

/// <summary>
///     Handles the logic for fulling a <see cref="HypnosisRequest"/>
/// </summary>
public class HypnosisHandler(
    IConnectionsService connections,
    IForwardedRequestManager forwardedRequestManager,
    ILogger<HypnosisHandler> logger)
{
    private const string Method = HubMethod.Hypnosis;
    private static readonly UserPermissions Permissions = new(PrimaryPermissions2.Hypnosis, SpeakPermissions2.None,
        ElevatedPermissions.None);

    /// <summary>
    ///     Handles the request
    /// </summary>
    public async Task<ActionResponse> Handle(string sender, HypnosisRequest request, IHubCallerClients clients)
    {
        if (connections.TryGetClient(sender) is not { } connectedClient)
        {
            logger.LogWarning("{Sender} tried to issue a command but is not in the connections list", sender);
            return new ActionResponse(ActionResponseEc.UnexpectedState);
        }

        if (connections.IsUserExceedingRequestLimit(connectedClient))
        {
            logger.LogWarning("{Sender} exceeded request limit", sender);
            return new ActionResponse(ActionResponseEc.TooManyRequests);
        }
        
        if (VerificationUtilities.IsValidListOfFriendCodes(request.TargetFriendCodes) is false)
        {
            logger.LogWarning("{Sender} sent invalid friend codes", sender);
            return new ActionResponse(ActionResponseEc.BadDataInRequest);
        }

        if (IsValidHypnosisRequest(request) is false)
        {
            logger.LogWarning("{Sender} sent invalid hypnosis data", sender);
            return new ActionResponse(ActionResponseEc.BadDataInRequest);
        }

        var forwardedRequest = new HypnosisForwardedRequest(sender, request.Data);
        return await forwardedRequestManager.CheckPermissionsAndSend(sender, request.TargetFriendCodes, Method,
            Permissions, forwardedRequest, clients);
    }

    private static bool IsValidHypnosisRequest(HypnosisRequest request)
    {
        if (request.Data.SpiralArms is < Constraints.Hypnosis.ArmsMin or > Constraints.Hypnosis.ArmsMax) return false;
        if (request.Data.SpiralTurns is < Constraints.Hypnosis.TurnsMin or > Constraints.Hypnosis.TurnsMax) return false;
        if (request.Data.SpiralCurve is < Constraints.Hypnosis.CurvesMin or > Constraints.Hypnosis.CurvesMax) return false;
        if (request.Data.SpiralThickness is < Constraints.Hypnosis.ThicknessMin or > Constraints.Hypnosis.ThicknessMax) return false;
        if (request.Data.SpiralSpeed is < Constraints.Hypnosis.SpeedMin or > Constraints.Hypnosis.SpeedMax) return false;
        if (request.Data.TextDelay is < Constraints.Hypnosis.TextDelayMin or > Constraints.Hypnosis.TextDelayMax) return false;
        if (request.Data.TextDuration is < Constraints.Hypnosis.TextDurationMin or > Constraints.Hypnosis.TextDurationMax) return false;

        var length = 0;
        foreach (var word in request.Data.TextWords)
            length += word.Length;
        
        if (length is < Constraints.Hypnosis.TextWordsMin or > Constraints.Hypnosis.TextWordsMax)
            return false;

        return true;
    }
}