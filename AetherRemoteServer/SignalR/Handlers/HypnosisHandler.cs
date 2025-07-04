using AetherRemoteCommon.Domain.Enums;
using AetherRemoteCommon.Domain.Enums.Permissions;
using AetherRemoteCommon.Domain.Network;
using AetherRemoteCommon.V2.Domain.Network;
using AetherRemoteCommon.V2.Domain.Network.Hypnosis;
using AetherRemoteServer.Domain;
using AetherRemoteServer.Domain.Interfaces;
using Microsoft.AspNetCore.SignalR;

namespace AetherRemoteServer.SignalR.Handlers;

public class HypnosisHandler(
    IConnectionsService connections,
    IForwardedRequestManager forwardedRequestManager,
    ILogger<HypnosisHandler> logger)
{
    private const string Method = HubMethod.Hypnosis;
    private const PrimaryPermissions2 Permissions = PrimaryPermissions2.Hypnosis;

    public async Task<ActionResponse> Handle(string sender, HypnosisRequest request, IHubCallerClients clients)
    {
        if (connections.IsUserExceedingRequestLimit(sender))
        {
            logger.LogWarning("{Friend} exceeded request limit", sender);
            return new ActionResponse(ActionResponseEc.TooManyRequests);
        }

        var forwardedRequest = new HypnosisForwardedRequest(sender, request.Spiral);
        var requestInfo = new PrimaryRequestInfo(Method, Permissions, forwardedRequest);
        return await forwardedRequestManager.Send(sender, request.TargetFriendCodes, requestInfo, clients);
    }
}