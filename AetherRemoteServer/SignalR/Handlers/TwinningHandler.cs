using AetherRemoteCommon.Domain;
using AetherRemoteCommon.Domain.Enums;
using AetherRemoteCommon.Domain.Enums.Permissions;
using AetherRemoteCommon.Domain.Network;
using AetherRemoteCommon.Domain.Network.Twinning;
using AetherRemoteCommon.Util;
using AetherRemoteServer.Domain.Interfaces;
using Microsoft.AspNetCore.SignalR;

namespace AetherRemoteServer.SignalR.Handlers;

/// <summary>
///     Handles the logic for fulfilling a <see cref="TwinningRequest"/>
/// </summary>
public class TwinningHandler(
    IConnectionsService connections,
    IForwardedRequestManager forwardedRequestManager,
    ILogger<AddFriendHandler> logger)
{
    private const string Method = HubMethod.Twinning;

    /// <summary>
    ///     Handle the request
    /// </summary>
    public async Task<ActionResponse> Handle(string sender, TwinningRequest request, IHubCallerClients clients)
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
        
        var primary = request.SwapAttributes.ToPrimaryPermission();
        if (primary == PrimaryPermissions2.None)
            logger.LogWarning("{Sender} tried to request with empty permissions {Request}", sender, request);
        
        var permissions = new UserPermissions(primary, SpeakPermissions2.None, ElevatedPermissions.None);
        
        var forwardedRequest = new TwinningForwardedRequest(sender, request.CharacterName, request.SwapAttributes);
        return await forwardedRequestManager.CheckPermissionsAndSend(sender, request.TargetFriendCodes, Method,
            permissions, forwardedRequest, clients);
    }
}