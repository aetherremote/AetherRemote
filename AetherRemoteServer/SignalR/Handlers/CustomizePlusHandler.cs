using AetherRemoteCommon.Domain;
using AetherRemoteCommon.Domain.Enums;
using AetherRemoteCommon.Domain.Enums.Permissions;
using AetherRemoteCommon.Domain.Network;
using AetherRemoteCommon.Domain.Network.Customize;
using AetherRemoteServer.Domain.Interfaces;
using AetherRemoteServer.Utilities;
using Microsoft.AspNetCore.SignalR;

namespace AetherRemoteServer.SignalR.Handlers;

public class CustomizePlusHandler(IPresenceService presenceService, IForwardedRequestManager forwardedRequestManager, ILogger<CustomizePlusHandler> logger)
{
    private const string Method = HubMethod.CustomizePlus;
    private static readonly UserPermissions Permissions = new(PrimaryPermissions2.CustomizePlus, SpeakPermissions2.None, ElevatedPermissions.None);

    /// <summary>
    ///     Handles the request
    /// </summary>
    public async Task<ActionResponse> Handle(string senderFriendCode, CustomizeRequest request, IHubCallerClients clients)
    {
        if (ValidateCustomizeRequest(senderFriendCode, request) is { } error)
        {
            logger.LogWarning("{Sender} sent invalid customize+ request {Error}", senderFriendCode, error);
            return new ActionResponse(error, []);
        }
        
        var forwardedRequest = new CustomizeCommand(senderFriendCode, request.JsonBoneDataBytes);
        return await forwardedRequestManager.CheckPermissionsAndSend(senderFriendCode, request.TargetFriendCodes, Method, Permissions, forwardedRequest, clients);
    }
    
    private ActionResponseEc? ValidateCustomizeRequest(string senderFriendCode, CustomizeRequest request)
    {
        if (presenceService.IsUserExceedingCooldown(senderFriendCode))
            return ActionResponseEc.TooManyRequests;
        
        if (VerificationUtilities.ValidListOfFriendCodes(request.TargetFriendCodes) is false)
            return ActionResponseEc.BadDataInRequest;

        if (VerificationUtilities.IsJsonBytes(request.JsonBoneDataBytes) is false)
            return ActionResponseEc.BadDataInRequest;
        
        return null;
    }
}