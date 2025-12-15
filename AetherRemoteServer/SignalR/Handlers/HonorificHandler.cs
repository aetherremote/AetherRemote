using AetherRemoteCommon.Domain;
using AetherRemoteCommon.Domain.Enums;
using AetherRemoteCommon.Domain.Enums.Permissions;
using AetherRemoteCommon.Domain.Network;
using AetherRemoteCommon.Domain.Network.Honorific;
using AetherRemoteServer.Domain.Interfaces;
using Microsoft.AspNetCore.SignalR;

namespace AetherRemoteServer.SignalR.Handlers;

/// <summary>
///     Handles the logic for fulling a <see cref="HonorificRequest"/>
/// </summary>
public class HonorificHandler(IConnectionsService connections, IForwardedRequestManager forwardedRequest, ILogger<MoodlesHandler> logger)
{
    private const string Method = HubMethod.Honorific;
    private static readonly UserPermissions Permissions = new(PrimaryPermissions2.Honorific, SpeakPermissions2.None, ElevatedPermissions.None);

    public async Task<ActionResponse> Handle(string senderFriendCode, HonorificRequest request, IHubCallerClients clients)
    {
        if (connections.TryGetClient(senderFriendCode) is not { } connectedClient)
        {
            logger.LogWarning("{Sender} tried to issue a command but is not in the connections list", senderFriendCode);
            return new ActionResponse(ActionResponseEc.UnexpectedState);
        }

        if (connections.IsUserExceedingRequestLimit(connectedClient))
        {
            logger.LogWarning("{Sender} exceeded request limit", senderFriendCode);
            return new ActionResponse(ActionResponseEc.TooManyRequests);
        }

        var forward = new HonorificForwardedRequest(senderFriendCode, request.Honorific);
        return await forwardedRequest.CheckPermissionsAndSend(senderFriendCode, request.TargetFriendCodes, Method, Permissions, forward, clients);
    }
}