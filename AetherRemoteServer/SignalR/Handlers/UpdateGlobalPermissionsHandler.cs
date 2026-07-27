using AetherRemoteCommon.Domain.Enums;
using AetherRemoteCommon.Domain.Network;
using AetherRemoteCommon.Network.Domain;
using AetherRemoteCommon.Network.Domain.Commands;
using AetherRemoteCommon.Network.Domain.Payloads;
using AetherRemoteCommon.Network.Enums;
using AetherRemoteCommon.Util;
using AetherRemoteServer.Domain.Interfaces;
using AetherRemoteServer.Infrastructure.Database;
using AetherRemoteServer.Services;
using Microsoft.AspNetCore.SignalR;

namespace AetherRemoteServer.SignalR.Handlers;

public class UpdateGlobalPermissionsHandler(
    ILogger<UpdateGlobalPermissionsHandler> logger,
    DatabaseInfrastructure databaseInfrastructure,
    SessionService sessionService) : ICommandHandler<UpdateGlobalPermissionsRequest, UpdateGlobalPermissionsResponse>
{
    public async Task<UpdateGlobalPermissionsResponse> Execute(string senderFriendCode, UpdateGlobalPermissionsRequest request, IHubCallerClients clients)
    {
        var databaseResultEc = await databaseInfrastructure.UpdateGlobalPermissions(senderFriendCode, request.Permissions);
        if (databaseResultEc is DatabaseResultEc.Unknown)
            return new UpdateGlobalPermissionsResponse(ResponseStatus.Unknown);
        
        var permissions = await databaseInfrastructure.GetAllPermissions(senderFriendCode);
        foreach (var permission in permissions)
        {
            // Ignore pending friends
            if (permission.PermissionsGrantedBy is null)
                continue;
            
            // Only evaluate online friends
            if (sessionService.GetSession(permission.TargetFriendCode) is not { } session)
                continue;
            
            try
            {
                var resolved = PermissionResolver.Resolve(request.Permissions, permission.PermissionsGrantedTo);
                var payload = new SyncPermissionsPayload(resolved);
                var message = new Message<SyncPermissionsPayload>(senderFriendCode, payload);
                await clients.Client(session.ConnectionId).SendAsync(HubMethod.SyncPermissions, message);
            }
            catch (Exception e)
            {
                logger.LogError("Syncing online status {Sender} -> {Target} failed, {Error}", senderFriendCode, permission.TargetFriendCode, e);
            }
        }

        return new UpdateGlobalPermissionsResponse(ResponseStatus.Success);
    }
}