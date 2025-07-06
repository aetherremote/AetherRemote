using AetherRemoteCommon.Domain;
using AetherRemoteCommon.Domain.Enums;
using AetherRemoteCommon.Domain.Enums.Permissions;
using AetherRemoteCommon.Domain.Network;
using AetherRemoteCommon.Domain.Network.Customize;
using AetherRemoteServer.Domain.Interfaces;
using Microsoft.AspNetCore.SignalR;

namespace AetherRemoteServer.SignalR.Handlers;

public class CustomizePlusHandler(
    IConnectionsService connections,
    IForwardedRequestManager forwardedRequestManager,
    ILogger<CustomizePlusHandler> logger)
{
    private const string Method = HubMethod.CustomizePlus;
    private static readonly UserPermissions Permissions = new(PrimaryPermissions2.CustomizePlus, SpeakPermissions2.None,
        ElevatedPermissions.None);

    /// <summary>
    ///     Handles the request
    /// </summary>
    public async Task<ActionResponse> Handle(string sender, CustomizeRequest request, IHubCallerClients clients)
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

        var forwardedRequest = new CustomizeForwardedRequest(sender, request.Data);
        return await forwardedRequestManager.CheckPermissionsAndSend(sender, request.TargetFriendCodes, Method,
            Permissions, forwardedRequest, clients);
    }
}