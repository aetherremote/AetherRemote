using AetherRemoteCommon;
using AetherRemoteCommon.Domain.Enums;
using AetherRemoteCommon.Domain.Enums.New;
using AetherRemoteCommon.Domain.Network;
using AetherRemoteCommon.V2.Domain.Enum;
using AetherRemoteCommon.V2.Domain.Network;
using AetherRemoteCommon.V2.Domain.Network.Speak;
using AetherRemoteServer.Domain;
using AetherRemoteServer.Domain.Interfaces;
using Microsoft.AspNetCore.SignalR;

namespace AetherRemoteServer.SignalR.Handlers;

/// <summary>
///     Handles the logic for fulfilling a <see cref="SpeakRequest"/>
/// </summary>
public class SpeakHandler(
    IConnectionsService connections,
    IForwardedRequestManager forwardedRequestManager,
    ILogger<SpeakHandler> logger)
{
    private const string Method = HubMethod.Speak;

    /// <summary>
    ///     Handles the request
    /// </summary>
    public async Task<ActionResponse> Handle(string sender, SpeakRequest request, IHubCallerClients clients)
    {
        if (connections.IsUserExceedingRequestLimit(sender))
        {
            logger.LogWarning("{Friend} exceeded request limit", sender);
            return new ActionResponse(ActionResponseEc.TooManyRequests);
        }

        if (request.TargetFriendCodes.Count > Constraints.MaximumTargetsForInGameOperations)
        {
            logger.LogWarning("{Friend} tried to target more than the allowed amount for in-game actions", sender);
            return new ActionResponse(ActionResponseEc.TooManyTargets);
        }

        var permissions = request.ChatChannel switch
        {
            ChatChannel.Say => SpeakPermissions2.Say,
            ChatChannel.Roleplay => SpeakPermissions2.Roleplay,
            ChatChannel.Echo => SpeakPermissions2.Echo,
            ChatChannel.Yell => SpeakPermissions2.Yell,
            ChatChannel.Shout => SpeakPermissions2.Shout,
            ChatChannel.Tell => SpeakPermissions2.Tell,
            ChatChannel.Party => SpeakPermissions2.Party,
            ChatChannel.Alliance => SpeakPermissions2.Alliance,
            ChatChannel.FreeCompany => SpeakPermissions2.FreeCompany,
            ChatChannel.PvPTeam => SpeakPermissions2.PvPTeam,
            ChatChannel.Linkshell => ConvertToLinkshell(SpeakPermissions2.Ls1, request.Extra),
            ChatChannel.CrossWorldLinkshell => ConvertToLinkshell(SpeakPermissions2.Cwl1, request.Extra),
            _ => SpeakPermissions2.None
        };

        if (permissions == SpeakPermissions2.None)
            return new ActionResponse(ActionResponseEc.BadDataInRequest);

        var forwardedRequest = new SpeakForwardedRequest(sender, request.Message, request.ChatChannel, request.Extra);
        var requestInfo = new SpeakRequestInfo(Method, permissions, forwardedRequest);
        return await forwardedRequestManager.Send(sender, request.TargetFriendCodes, requestInfo, clients);
    }

    private static SpeakPermissions2 ConvertToLinkshell(SpeakPermissions2 starting, string? extra)
    {
        return int.TryParse(extra, out var number)
            ? (SpeakPermissions2)((int)starting << (number - 1))
            : SpeakPermissions2.None;
    }
}