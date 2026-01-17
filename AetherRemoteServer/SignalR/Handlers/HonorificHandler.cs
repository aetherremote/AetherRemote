using AetherRemoteCommon.Domain;
using AetherRemoteCommon.Domain.Enums;
using AetherRemoteCommon.Domain.Enums.Permissions;
using AetherRemoteCommon.Domain.Network;
using AetherRemoteCommon.Domain.Network.Honorific;
using AetherRemoteServer.Domain.Interfaces;
using AetherRemoteServer.Utilities;
using Microsoft.AspNetCore.SignalR;

namespace AetherRemoteServer.SignalR.Handlers;

/// <summary>
///     Handles the logic for fulling a <see cref="HonorificRequest"/>
/// </summary>
public class HonorificHandler(IPresenceService presenceService, IForwardedRequestManager forwardedRequest, ILogger<MoodlesHandler> logger)
{
    private const string Method = HubMethod.Honorific;
    private static readonly UserPermissions Permissions = new(PrimaryPermissions2.Honorific, SpeakPermissions2.None, ElevatedPermissions.None);

    public async Task<ActionResponse> Handle(string senderFriendCode, HonorificRequest request, IHubCallerClients clients)
    {
        if (ValidateHonorificRequest(senderFriendCode, request) is { } error)
        {
            logger.LogWarning("{Sender} sent invalid speak request", senderFriendCode);
            return new ActionResponse(error, []);
        }

        var command = new HonorificCommand(senderFriendCode, request.Honorific);
        return await forwardedRequest.CheckPermissionsAndSend(senderFriendCode, request.TargetFriendCodes, Method, Permissions, command, clients);
    }
    
    private ActionResponseEc? ValidateHonorificRequest(string senderFriendCode, HonorificRequest request)
    {
        if (presenceService.IsUserExceedingCooldown(senderFriendCode))
            return ActionResponseEc.TooManyRequests;
        
        if (VerificationUtilities.ValidListOfFriendCodes(request.TargetFriendCodes) is false)
            return ActionResponseEc.BadDataInRequest;
        
        // TODO: Define rules for validating Honorific data
        
        return null;
    }
}