using AetherRemoteCommon;
using AetherRemoteCommon.Domain;
using AetherRemoteCommon.Domain.Enums;
using AetherRemoteCommon.Domain.Enums.Permissions;
using AetherRemoteCommon.Domain.Network;
using AetherRemoteCommon.Domain.Network.Speak;
using AetherRemoteCommon.Util;
using AetherRemoteServer.Domain.Interfaces;
using AetherRemoteServer.Utilities;
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

        if (request.TargetFriendCodes.Count > Constraints.MaximumTargetsForInGameOperations)
        {
            logger.LogWarning("{Sender} tried to target more than the allowed amount for in-game actions", sender);
            return new ActionResponse(ActionResponseEc.TooManyTargets);
        }

        if (IsValidSpeakRequest(request) is false)
        {
            logger.LogWarning("{Sender} sent invalid speak data", sender);
            return new ActionResponse(ActionResponseEc.BadDataInRequest);
        }
        
        var speak = request.ChatChannel.ToSpeakPermissions(request.Extra);
        if (speak == SpeakPermissions2.None)
            logger.LogWarning("{Sender} tried to request with empty permissions {Request}", sender, request);
        
        var permissions = new UserPermissions(PrimaryPermissions2.None, speak, ElevatedPermissions.None);

        var forwardedRequest = new SpeakForwardedRequest(sender, request.Message, request.ChatChannel, request.Extra);
        return await forwardedRequestManager.CheckPermissionsAndSend(sender, request.TargetFriendCodes, Method,
            permissions, forwardedRequest, clients);
    }
    
    private static bool IsValidSpeakRequest(SpeakRequest request)
    {
        if (request.Message.Length is < Constraints.Speak.MessageMin or > Constraints.Speak.MessageMax)
            return false;

        if (request.Extra?.Length is < Constraints.Speak.MessageExtraMin or > Constraints.Speak.MessageExtraMax)
            return false;

        return true;
    }
}