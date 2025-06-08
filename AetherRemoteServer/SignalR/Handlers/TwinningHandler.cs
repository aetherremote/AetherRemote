using AetherRemoteCommon.Domain.Enums;
using AetherRemoteCommon.Domain.Enums.New;
using AetherRemoteCommon.Domain.Network;
using AetherRemoteCommon.Util;
using AetherRemoteCommon.V2.Domain.Enum;
using AetherRemoteCommon.V2.Domain.Network;
using AetherRemoteCommon.V2.Domain.Network.Twinning;
using AetherRemoteServer.Domain;
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
    private const string Method = HubMethod.Hypnosis;

    /// <summary>
    ///     Handle the request
    /// </summary>
    public async Task<ActionResponse> Handle(string sender, TwinningRequest request, IHubCallerClients clients)
    {
        if (connections.IsUserExceedingRequestLimit(sender))
        {
            logger.LogWarning("{Friend} exceeded request limit", sender);
            return new ActionResponse(ActionResponseEc.TooManyRequests);
        }

        var permissions = request.SwapAttributes.ToPrimaryPermission();
        var forwardedRequest = new TwinningForwardedRequest(sender, request.CharacterName, request.SwapAttributes);
        var requestInfo = new PrimaryRequestInfo(Method, permissions, forwardedRequest);
        return await forwardedRequestManager.Send(sender, request.TargetFriendCodes, requestInfo, clients);
    }
}