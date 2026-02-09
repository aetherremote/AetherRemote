using AetherRemoteCommon.Domain;
using AetherRemoteCommon.Domain.Enums;
using AetherRemoteCommon.Domain.Enums.Permissions;
using AetherRemoteCommon.Domain.Network;
using AetherRemoteCommon.Domain.Network.Honorific;
using AetherRemoteServer.Managers;
using AetherRemoteServer.Services;
using AetherRemoteServer.Utilities;
using Microsoft.AspNetCore.SignalR;

namespace AetherRemoteServer.SignalR.Handlers;

/// <summary>
///     Handles the logic for fulling a <see cref="HonorificRequest"/>
/// </summary>
public class HonorificHandler(PresenceService presenceService, ForwardedRequestManager forwardedRequest, ILogger<MoodlesHandler> logger)
{
    private const string Method = HubMethod.Honorific;
    private static readonly ResolvedPermissions Permissions = new(PrimaryPermissions.Honorific, SpeakPermissions.None, ElevatedPermissions.None);

    public async Task<ActionResponse> Handle(string senderFriendCode, HonorificRequest request, IHubCallerClients clients)
    {
        if (ValidateHonorificRequest(senderFriendCode, request) is { } error)
        {
            logger.LogWarning("{Sender} sent invalid speak request {Error}", senderFriendCode, error);
            return new ActionResponse(error, []);
        }

        var command = new HonorificCommand(senderFriendCode, request.Honorific);
        return await forwardedRequest.CheckPermissionsAndSend(senderFriendCode, request.TargetFriendCodes, Method, Permissions, command, clients);
    }
    
    private ActionResponseEc? ValidateHonorificRequest(string senderFriendCode, HonorificRequest request)
    {
        if (presenceService.IsUserExceedingCooldown(senderFriendCode))
            return ActionResponseEc.TooManyRequests;
        
        if (VerificationUtilities.ValidFriendCodes(request.TargetFriendCodes) is false)
            return ActionResponseEc.BadDataInRequest;
        
        // TODO: Define rules for validating Honorific data
        
        return null;
    }
}