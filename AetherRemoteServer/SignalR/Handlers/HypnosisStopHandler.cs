using AetherRemoteCommon.Domain;
using AetherRemoteCommon.Domain.Enums;
using AetherRemoteCommon.Domain.Enums.Permissions;
using AetherRemoteCommon.Domain.Network;
using AetherRemoteCommon.Domain.Network.HypnosisStop;
using AetherRemoteServer.Domain.Interfaces;
using AetherRemoteServer.Utilities;
using Microsoft.AspNetCore.SignalR;

namespace AetherRemoteServer.SignalR.Handlers;

public class HypnosisStopHandler(IConnectionsService connections, IForwardedRequestManager forwardedRequestManager, ILogger<HypnosisHandler> logger)
{
    private const string Method = HubMethod.HypnosisStop;
    private static readonly UserPermissions Permissions = new(PrimaryPermissions2.Hypnosis, SpeakPermissions2.None, ElevatedPermissions.None);
    
    public async Task<ActionResponse> Handle(string sender, HypnosisStopRequest request, IHubCallerClients clients)
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
        
        var forwardedRequest = new HypnosisStopForwardedRequest(sender);
        return await forwardedRequestManager.CheckPermissionsAndSend(sender, request.TargetFriendCodes, Method, Permissions, forwardedRequest, clients);
    }
}