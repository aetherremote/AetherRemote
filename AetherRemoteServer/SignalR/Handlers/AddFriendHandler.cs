using AetherRemoteCommon.Domain;
using AetherRemoteCommon.Domain.Enums;
using AetherRemoteCommon.Domain.Enums.Permissions;
using AetherRemoteCommon.Domain.Network;
using AetherRemoteCommon.Domain.Network.SyncOnlineStatus;
using AetherRemoteCommon.V2.Network.Commands;
using AetherRemoteServer.Infrastructure.Database;
using AetherRemoteServer.Services;
using AetherRemoteServer.SignalR.Handlers.Base;
using Microsoft.AspNetCore.SignalR;

namespace AetherRemoteServer.SignalR.Handlers;

public class AddFriendHandler(
    ILogger<AddFriendHandler> logger, 
    DatabaseInfrastructure databaseInfrastructure, 
    PresenceService presenceService) : ICommandHandler<AddFriendRequest, AddFriendResponse>
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
        
        if (presenceService.TryGet(request.TargetFriendCode) is not { } target)
            return new AddFriendResponse(code, FriendOnlineStatus.Offline);
        
        try
        {
            // Try to send an update to that client that we've accepted the friend request
            var sync = new SyncOnlineStatusCommand(senderFriendCode, FriendOnlineStatus.Online, EmptyPermissions);
            await clients.Client(target.ConnectionId).SendAsync(HubMethod.SyncOnlineStatus, sync);
        }
        catch (Exception e)
        {
            logger.LogError("Syncing online status {Sender} -> {Target} failed, {Error}", senderFriendCode, request.TargetFriendCode, e);
        }
        
        return new AddFriendResponse(code, FriendOnlineStatus.Online);
    }
}