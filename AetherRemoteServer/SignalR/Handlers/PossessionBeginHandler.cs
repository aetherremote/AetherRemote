using AetherRemoteCommon.Domain;
using AetherRemoteCommon.Domain.Enums.Permissions;
using AetherRemoteCommon.Domain.Network;
using AetherRemoteCommon.Domain.Network.Possession;
using AetherRemoteCommon.Domain.Network.Possession.Begin;
using AetherRemoteServer.Domain;
using AetherRemoteServer.Managers;
using AetherRemoteServer.Services;
using AetherRemoteServer.Utilities;
using Microsoft.AspNetCore.SignalR;

namespace AetherRemoteServer.SignalR.Handlers;

/// <summary>
///     Processes entering into a possession session
/// </summary>
/// <param name="presences"></param>
/// <param name="forwarder"></param>
/// <param name="possessionManager"></param>
/// <param name="logger"></param>
public class PossessionBeginHandler(
    PresenceService presences, 
    ForwardedRequestManager forwarder, 
    PossessionManager possessionManager, 
    ILogger<PossessionBeginHandler> logger)
{
    private const string Method = HubMethod.Possession.Begin;
    private static readonly ResolvedPermissions Required = new(PrimaryPermissions2.None, SpeakPermissions2.None, ElevatedPermissions.Possession);
    
    public async Task<PossessionBeginResponse> Handle(string senderFriendCode, PossessionBeginRequest request, IHubCallerClients clients)
    {
        if (ValidatePossessionBeginRequest(senderFriendCode, request) is { } error)
        {
            logger.LogWarning("{Sender} sent invalid possession begin request {Error}", senderFriendCode, error);
            return new PossessionBeginResponse(error, PossessionResultEc.Uninitialized, string.Empty, string.Empty);
        }
        
        var command = new PossessionBeginCommand(senderFriendCode, request.MoveMode);
        var response = await forwarder.CheckPossessionAndInvoke(senderFriendCode, request.TargetFriendCode, Method, Required, command, clients);

        // If the response or the result is not success, just return the response object
        if (response.Response is not PossessionResponseEc.Success || response.Result is not PossessionResultEc.Success)
            return new PossessionBeginResponse(response.Response, response.Result, string.Empty, string.Empty);

        if (presences.TryGet(request.TargetFriendCode) is not { } target)
            return new PossessionBeginResponse(PossessionResponseEc.TargetOffline, PossessionResultEc.Uninitialized, string.Empty, string.Empty);
        
        var session = new Session(senderFriendCode, request.TargetFriendCode);
        possessionManager.TryAddSession(senderFriendCode, request.TargetFriendCode, session);
        return new PossessionBeginResponse(response.Response, response.Result, target.CharacterName, target.CharacterWorld);
    }
    
    private PossessionResponseEc? ValidatePossessionBeginRequest(string senderFriendCode, PossessionBeginRequest request)
    {
        if (presences.IsUserExceedingCooldown(senderFriendCode))
            return PossessionResponseEc.TooManyRequests;
        
        if (VerificationUtilities.ValidFriendCode(request.TargetFriendCode) is false)
            return PossessionResponseEc.BadDataInRequest;

        if (possessionManager.TryGetSession(senderFriendCode) is not null)
            return PossessionResponseEc.SenderAlreadyInSession;
        
        if (possessionManager.TryGetSession(request.TargetFriendCode) is not null)
            return  PossessionResponseEc.TargetAlreadyInSession;

        if (request.MoveMode > 1)
            return PossessionResponseEc.BadDataInRequest;

        return null;
    }
}