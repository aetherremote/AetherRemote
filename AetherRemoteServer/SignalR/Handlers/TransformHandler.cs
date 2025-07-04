using AetherRemoteCommon.Domain.Enums;
using AetherRemoteCommon.Domain.Enums.Permissions;
using AetherRemoteCommon.Domain.Network;
using AetherRemoteCommon.Util;
using AetherRemoteCommon.V2.Domain.Network;
using AetherRemoteCommon.V2.Domain.Network.Transform;
using AetherRemoteServer.Domain;
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
        if (connections.IsUserExceedingRequestLimit(sender))
        {
            logger.LogWarning("{Friend} exceeded request limit", sender);
            return new ActionResponse(ActionResponseEc.TooManyRequests);
        }

        var permissions = request.GlamourerApplyType.ToPrimaryPermission();
        if (permissions == PrimaryPermissions2.Twinning)
            return new ActionResponse(ActionResponseEc.BadDataInRequest);
        
        var forwardedRequest = new TransformForwardedRequest(sender, request.GlamourerData, request.GlamourerApplyType);
        var requestInfo = new PrimaryRequestInfo(Method, permissions, forwardedRequest);
        return await forwardedRequestManager.Send(sender, request.TargetFriendCodes, requestInfo, clients);
    }
}