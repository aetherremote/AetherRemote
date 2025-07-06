using AetherRemoteCommon.Domain.Enums;
using AetherRemoteCommon.Domain.Enums.Permissions;
using AetherRemoteCommon.Domain.Network;
using AetherRemoteCommon.Domain.Network.Hypnosis;
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
    
    public async Task<ActionResponse> Handle2(string sender, HypnosisRequest request, IHubCallerClients clients)
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

        Mlp.CheckPermissions("", "", Permissions);
        

        var forwardedRequest = new HypnosisForwardedRequest(sender, request.Spiral);
        var requestInfo = new PrimaryRequestInfo(Method, Permissions, forwardedRequest);
        return await forwardedRequestManager.Send(sender, request.TargetFriendCodes, requestInfo, clients);
    }
}

public static class Mlp
{
    public static async Task<ActionResult<bool>> CheckPermissions<T>(string sender, string target, T permissions) where T : Enum
    {
        return new ActionResult<bool>();
    }
}