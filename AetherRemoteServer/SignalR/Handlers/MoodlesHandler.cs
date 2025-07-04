using AetherRemoteCommon.Domain.Enums;
using AetherRemoteCommon.Domain.Enums.Permissions;
using AetherRemoteCommon.Domain.Network;
using AetherRemoteCommon.V2.Domain.Network;
using AetherRemoteCommon.V2.Domain.Network.Moodles;
using AetherRemoteServer.Domain;
using AetherRemoteServer.Domain.Interfaces;
using Microsoft.AspNetCore.SignalR;

namespace AetherRemoteServer.SignalR.Handlers;

/// <summary>
///     Handles the logic for fulling a <see cref="MoodlesRequest"/>
/// </summary>
public class MoodlesHandler(
    IConnectionsService connections,
    IForwardedRequestManager forwardedRequestManager,
    ILogger<MoodlesHandler> logger)
{
    private const string Method = HubMethod.Moodles;
    private const PrimaryPermissions2 Permissions = PrimaryPermissions2.Moodles;
    
    /// <summary>
    ///     Handles the request
    /// </summary>
    public async Task<ActionResponse> Handle(string sender, MoodlesRequest request, IHubCallerClients clients)
    {
        if (connections.IsUserExceedingRequestLimit(sender))
        {
            logger.LogWarning("{Friend} exceeded request limit", sender);
            return new ActionResponse(ActionResponseEc.TooManyRequests);
        }

        var forwardedRequest = new MoodlesForwardedRequest(sender, request.Moodle);
        var requestInfo = new PrimaryRequestInfo(Method, Permissions, forwardedRequest);
        return await forwardedRequestManager.Send(sender, request.TargetFriendCodes, requestInfo, clients);
    }
}