using AetherRemoteCommon;
using AetherRemoteCommon.Domain;
using AetherRemoteCommon.Domain.Enums;
using AetherRemoteCommon.Domain.Enums.Permissions;
using AetherRemoteCommon.Domain.Network;
using AetherRemoteCommon.Domain.Network.Emote;
using AetherRemoteServer.Domain.Interfaces;
using AetherRemoteServer.Utilities;
using Microsoft.AspNetCore.SignalR;
using Constraints = AetherRemoteCommon.Constraints;

namespace AetherRemoteServer.SignalR.Handlers;

/// <summary>
///     Handles the logic for fulfilling a <see cref="EmoteRequest"/>
/// </summary>
public class EmoteHandler(
    IConnectionsService connections,
    IForwardedRequestManager forwardedRequestManager,
    ILogger<EmoteHandler> logger)
{
    private const string Method = HubMethod.Emote;
    private static readonly UserPermissions Permissions = new(PrimaryPermissions2.Emote, SpeakPermissions2.None,
        ElevatedPermissions.None);
    
    /// <summary>
    ///     Handles the request
    /// </summary>
    public async Task<ActionResponse> Handle(string sender, EmoteRequest request, IHubCallerClients clients)
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
        
        var forwardedRequest = new EmoteForwardedRequest(sender, request.Emote, request.DisplayLogMessage);
        return await forwardedRequestManager.CheckPermissionsAndSend(sender, request.TargetFriendCodes, Method,
            Permissions, forwardedRequest, clients);
    }
}