using AetherRemoteCommon.Domain;
using AetherRemoteCommon.Domain.Enums;
using AetherRemoteCommon.Domain.Enums.Permissions;
using AetherRemoteCommon.Domain.Network;
using AetherRemoteCommon.Domain.Network.Twinning;
using AetherRemoteCommon.Util;
using AetherRemoteServer.Utilities;
using Microsoft.AspNetCore.SignalR;

namespace AetherRemoteServer.SignalR.Handlers.Test;

public partial class RequestHandler
{
    /// <summary>
    ///     Handles the logic for fulfilling a <see cref="TwinningRequest"/>
    /// </summary>
    public async Task<ActionResponse> HandleTwinning(string senderFriendCode, TwinningRequest request, IHubCallerClients clients)
    {
        if (ValidateEmoteRequest(senderFriendCode, request) is { } error)
        {
            _logger.LogWarning("{Sender} sent invalid twinning request {Error}", senderFriendCode, error);
            return new ActionResponse(error, []);
        }
        
        var primary = request.SwapAttributes.ToPrimaryPermissions();
        primary |= PrimaryPermissions.Twinning;
        
        var elevated = ElevatedPermissions.None;
        if (request.LockCode is not null)
            elevated = ElevatedPermissions.PermanentTransformation;
        
        var permissions = new ResolvedPermissions(primary, SpeakPermissions.None, elevated);
        var command = new TwinningCommand(senderFriendCode, request.CharacterName, request.CharacterWorld, request.SwapAttributes, request.LockCode);
        return await _forwardedRequestManager.CheckPermissionsAndSend(
            senderFriendCode, 
            request.TargetFriendCodes,
            HubMethod.Twinning, 
            permissions, 
            command, 
            clients);
    }
    
    private ActionResponseEc? ValidateEmoteRequest(string senderFriendCode, TwinningRequest request)
    {
        if (_presenceService.IsUserExceedingCooldown(senderFriendCode))
            return ActionResponseEc.TooManyRequests;
        
        if (VerificationUtilities.ValidFriendCodes(request.TargetFriendCodes) is false)
            return ActionResponseEc.BadDataInRequest;

        return null;
    }
}