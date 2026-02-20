using AetherRemoteCommon.Domain;
using AetherRemoteCommon.Domain.Enums.Permissions;
using AetherRemoteCommon.Domain.Network;
using AetherRemoteCommon.Domain.Network.Possession;
using AetherRemoteCommon.Domain.Network.Possession.Begin;
using AetherRemoteServer.Domain;
using AetherRemoteServer.Utilities;
using Microsoft.AspNetCore.SignalR;

namespace AetherRemoteServer.SignalR.Handlers.Test;

public partial class RequestHandler
{
    public async Task<PossessionBeginResponse> HandlePossessionBegin(string senderFriendCode, PossessionBeginRequest request, IHubCallerClients clients)
    {
        if (ValidatePossessionBeginRequest(senderFriendCode, request) is { } error)
        {
            _logger.LogWarning("{Sender} sent invalid possession begin request {Error}", senderFriendCode, error);
            return new PossessionBeginResponse(error, PossessionResultEc.Uninitialized, string.Empty, string.Empty);
        }
        
        var command = new PossessionBeginCommand(senderFriendCode, request.MoveMode);
        var response = await _forwardedRequestManager.CheckPossessionAndInvoke(
            senderFriendCode, 
            request.TargetFriendCode, 
            HubMethod.Possession.Begin, 
            new ResolvedPermissions(PrimaryPermissions.None, SpeakPermissions.None, ElevatedPermissions.Possession),
            command, 
            clients);

        // If the response or the result is not success, just return the response object
        if (response.Response is not PossessionResponseEc.Success || response.Result is not PossessionResultEc.Success)
            return new PossessionBeginResponse(response.Response, response.Result, string.Empty, string.Empty);

        if (_presenceService.TryGet(request.TargetFriendCode) is not { } target)
            return new PossessionBeginResponse(PossessionResponseEc.TargetOffline, PossessionResultEc.Uninitialized, string.Empty, string.Empty);
        
        var session = new Session(senderFriendCode, request.TargetFriendCode);
        _possessionManager.TryAddSession(senderFriendCode, request.TargetFriendCode, session);
        return new PossessionBeginResponse(response.Response, response.Result, target.CharacterName, target.CharacterWorld);
    }
    
    private PossessionResponseEc? ValidatePossessionBeginRequest(string senderFriendCode, PossessionBeginRequest request)
    {
        if (_presenceService.IsUserExceedingCooldown(senderFriendCode))
            return PossessionResponseEc.TooManyRequests;
        
        if (VerificationUtilities.ValidFriendCode(request.TargetFriendCode) is false)
            return PossessionResponseEc.BadDataInRequest;

        if (_possessionManager.TryGetSession(senderFriendCode) is not null)
            return PossessionResponseEc.SenderAlreadyInSession;
        
        if (_possessionManager.TryGetSession(request.TargetFriendCode) is not null)
            return  PossessionResponseEc.TargetAlreadyInSession;

        if (request.MoveMode > 1)
            return PossessionResponseEc.BadDataInRequest;

        return null;
    }
}