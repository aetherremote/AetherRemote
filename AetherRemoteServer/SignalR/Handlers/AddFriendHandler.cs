using AetherRemoteCommon.Domain;
using AetherRemoteCommon.Domain.Enums;
using AetherRemoteCommon.Domain.Enums.Permissions;
using AetherRemoteCommon.Domain.Network;
using AetherRemoteCommon.Network.Domain;
using AetherRemoteCommon.Network.Domain.Commands;
using AetherRemoteCommon.Network.Domain.Payloads;
using AetherRemoteCommon.Network.Enums.ErrorCodes;
using AetherRemoteServer.Domain.Interfaces;
using AetherRemoteServer.Infrastructure.Database;
using AetherRemoteServer.Services;
using Microsoft.AspNetCore.SignalR;

namespace AetherRemoteServer.SignalR.Handlers;

public class AddFriendHandler(
    ILogger<AddFriendHandler> logger, 
    DatabaseInfrastructure databaseInfrastructure, 
    SessionService sessionService) : ICommandHandler<AddFriendRequest, AddFriendResponse>
{
    private static readonly ResolvedPermissions EmptyPermissions = new(PrimaryPermissions.None, SpeakPermissions.None, ElevatedPermissions.None);
    
    public async Task<AddFriendResponse> Execute(string senderFriendCode, AddFriendRequest request, IHubCallerClients clients)
    {
        var code = await databaseInfrastructure.CreatePermissions(senderFriendCode, request.TargetFriendCode) switch
        {
            DatabaseResultEc.Success => AddFriendEc.Success,
            DatabaseResultEc.Pending => AddFriendEc.Pending,
            DatabaseResultEc.AlreadyFriends => AddFriendEc.AlreadyFriends,
            DatabaseResultEc.NoSuchFriendCode => AddFriendEc.NoSuchFriendCode,
            _ => AddFriendEc.Unknown
        };
        
        // Only send a message to the other client if the code was successful
        if (code is not AddFriendEc.Success)
        {
            return code is AddFriendEc.Pending 
                ? new AddFriendResponse(code, FriendOnlineStatus.Pending) 
                : new AddFriendResponse(code, FriendOnlineStatus.Offline);
        }
        
        if (sessionService.GetSession(request.TargetFriendCode) is not { } session)
            return new AddFriendResponse(code, FriendOnlineStatus.Offline);
        
        try
        {
            var payload = new SyncOnlineStatusPayload(FriendOnlineStatus.Online, EmptyPermissions);
            var message = new Message<SyncOnlineStatusPayload>(senderFriendCode, payload);
            await clients.Client(session.ConnectionId).SendAsync(HubMethod.SyncOnlineStatus, message);
        }
        catch (Exception e)
        {
            logger.LogError("Syncing online status {Sender} -> {Target} failed, {Error}", senderFriendCode, request.TargetFriendCode, e);
        }
        
        return new AddFriendResponse(code, FriendOnlineStatus.Online);
    }
}