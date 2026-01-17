using AetherRemoteCommon.Domain;
using AetherRemoteCommon.Domain.Enums;
using AetherRemoteCommon.Domain.Enums.Permissions;
using AetherRemoteCommon.Domain.Network;
using AetherRemoteCommon.Domain.Network.HypnosisStop;
using AetherRemoteServer.Domain.Interfaces;
using AetherRemoteServer.Utilities;
using Microsoft.AspNetCore.SignalR;

namespace AetherRemoteServer.SignalR.Handlers;

public class HypnosisStopHandler(IPresenceService presenceService, IForwardedRequestManager forwardedRequestManager, ILogger<HypnosisHandler> logger)
{
    private const string Method = HubMethod.HypnosisStop;
    private static readonly UserPermissions Permissions = new(PrimaryPermissions2.Hypnosis, SpeakPermissions2.None, ElevatedPermissions.None);
    
    public async Task<ActionResponse> Handle(string senderFriendCode, HypnosisStopRequest request, IHubCallerClients clients)
    {
        if (ValidateEmoteRequest(senderFriendCode, request) is { } error)
        {
            logger.LogWarning("{Sender} sent invalid hypnosis stop request", senderFriendCode);
            return new ActionResponse(error, []);
        }
        
        var command = new HypnosisStopCommand(senderFriendCode);
        return await forwardedRequestManager.CheckPermissionsAndSend(senderFriendCode, request.TargetFriendCodes, Method, Permissions, command, clients);
    }
    
    private ActionResponseEc? ValidateEmoteRequest(string senderFriendCode, HypnosisStopRequest request)
    {
        if (presenceService.IsUserExceedingCooldown(senderFriendCode))
            return ActionResponseEc.TooManyRequests;
        
        if (VerificationUtilities.ValidListOfFriendCodes(request.TargetFriendCodes) is false)
            return ActionResponseEc.BadDataInRequest;

        return null;
    }
}