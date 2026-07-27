using AetherRemoteCommon.Domain.Enums;
using AetherRemoteCommon.Domain.Network;
using AetherRemoteCommon.Network.Domain;
using AetherRemoteCommon.Network.Domain.Commands;
using AetherRemoteCommon.Network.Domain.Payloads;
using AetherRemoteCommon.Network.Enums.ErrorCodes;
using AetherRemoteServer.Domain.Interfaces;
using AetherRemoteServer.Infrastructure.Database;
using AetherRemoteServer.Services;
using AetherRemoteServer.Utilities;
using Microsoft.AspNetCore.SignalR;

namespace AetherRemoteServer.SignalR.Handlers;

public class RemoveFriendHandler(
    ILogger<RemoveFriendHandler> logger,
    DatabaseInfrastructure databaseInfrastructure,
    SessionService sessionService) : ICommandHandler<RemoveFriendRequest, RemoveFriendResponse>
{
    public async Task<RemoveFriendResponse> Execute(string senderFriendCode, RemoveFriendRequest request, IHubCallerClients clients)
    {
        if (ValidateRequest(senderFriendCode, request) is { } error)
        {
            logger.LogWarning("{Sender} sent invalid Remove Friend request {Error}", senderFriendCode, error);
            return new RemoveFriendResponse(error);
        }
        
        var result = await databaseInfrastructure.DeletePermissions(senderFriendCode, request.TargetFriendCode) switch
        {
            // TODO: ??? What does NoOp have to do with NotFriends
            DatabaseResultEc.NoOp => RemoveFriendEc.NotFriends,
            DatabaseResultEc.Success => RemoveFriendEc.Success,
            _ => RemoveFriendEc.Unknown
        };
        
        if (result is not RemoveFriendEc.Success)
            return new RemoveFriendResponse(result);
        
        if (sessionService.GetSession(request.TargetFriendCode) is not { } session)
            return new RemoveFriendResponse(result);
        
        if (await databaseInfrastructure.GetSinglePermissions(request.TargetFriendCode, senderFriendCode) is null)
            return new RemoveFriendResponse(result);

        try
        {
            var payload = new SyncOnlineStatusPayload(FriendOnlineStatus.Pending);
            var message = new Message<SyncOnlineStatusPayload>(senderFriendCode, payload);
            await clients.Client(session.ConnectionId).SendAsync(HubMethod.SyncOnlineStatus, message);
        }
        catch (Exception e)
        {
            logger.LogError("Syncing online status {Sender} -> {Target} failed, {Error}", senderFriendCode, request.TargetFriendCode, e);
        }
        
        return new RemoveFriendResponse(result);
    }
    
    private RemoveFriendEc? ValidateRequest(string senderFriendCode, RemoveFriendRequest request)
    {
        if (sessionService.GetSession(senderFriendCode)?.GeneralBucket.TryConsumeToken() is not true)
            return RemoveFriendEc.TooManyRequests;
        
        if (VerificationUtilities.ValidFriendCode(request.TargetFriendCode) is false)
            return RemoveFriendEc.BadRequest;
        
        return null;
    }
}