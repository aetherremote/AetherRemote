using AetherRemoteCommon.Domain;
using AetherRemoteCommon.Domain.Enums;
using AetherRemoteCommon.Domain.Enums.Permissions;
using AetherRemoteCommon.Domain.Network;
using AetherRemoteCommon.Domain.Network.Transform;
using AetherRemoteCommon.Util;
using AetherRemoteServer.Domain.Interfaces;
using Microsoft.AspNetCore.SignalR;

namespace AetherRemoteServer.SignalR.Handlers;

/// <summary>
///     Handles the logic for fulfilling a <see cref="TransformRequest"/>
/// </summary>
public class TransformHandler(
    IConnectionsService connections,
    IForwardedRequestManager forwardedRequestManager,
    ILogger<AddFriendHandler> logger)
{
    private const string Method = HubMethod.Transform;

    /// <summary>
    ///     Handle the request
    /// </summary>
    public async Task<ActionResponse> Handle(string sender, TransformRequest request, IHubCallerClients clients)
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

        var primary = request.GlamourerApplyType.ToPrimaryPermission();
        if (primary == PrimaryPermissions2.None)
            logger.LogWarning("{Sender} tried to request with empty permissions {Request}", sender, request);

        var elevated = ElevatedPermissions.None;
        if (request.LockCode is not null)
            elevated = ElevatedPermissions.PermanentTransformation;

        var permissions = new UserPermissions(primary, SpeakPermissions2.None, elevated);
        
        var forwardedRequest = new TransformForwardedRequest(sender, request.GlamourerData, request.GlamourerApplyType, request.LockCode);
        return await forwardedRequestManager.CheckPermissionsAndSend(sender, request.TargetFriendCodes, Method,
            permissions, forwardedRequest, clients);
    }
}