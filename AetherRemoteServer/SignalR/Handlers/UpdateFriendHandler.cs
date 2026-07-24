using AetherRemoteCommon.Domain.Enums;
using AetherRemoteCommon.Domain.Network;
using AetherRemoteCommon.Network.Domain;
using AetherRemoteCommon.Network.Domain.Commands;
using AetherRemoteCommon.Network.Domain.Payloads;
using AetherRemoteCommon.Network.Enums.ErrorCodes;
using AetherRemoteCommon.Util;
using AetherRemoteServer.Domain.Interfaces;
using AetherRemoteServer.Infrastructure.Database;
using AetherRemoteServer.Services;
using Microsoft.AspNetCore.SignalR;

namespace AetherRemoteServer.SignalR.Handlers;

public class UpdateFriendHandler(
    ILogger<UpdateFriendHandler> logger,
    DatabaseInfrastructure databaseInfrastructure,
    SessionService sessionService) : ICommandHandler<UpdateFriendRequest, UpdateFriendResponse>
{
    public async Task<UpdateFriendResponse> Execute(string senderFriendCode, UpdateFriendRequest request, IHubCallerClients clients)
    {
        var databaseResult = await databaseInfrastructure.UpdatePermissions(senderFriendCode, request.TargetFriendCode, request.Permissions);
        var result = databaseResult switch
        {
            DatabaseResultEc.Success => UpdateFriendEc.Success,
            DatabaseResultEc.NoOp => UpdateFriendEc.NoOp,
            _ => UpdateFriendEc.Unknown
        };
        
        if (sessionService.GetSession(request.TargetFriendCode) is not { } session)
            return new UpdateFriendResponse(result);
        
        // TODO: Update failure state. This is not an expected state
        if (await databaseInfrastructure.GetGlobalPermissions(senderFriendCode) is not { } global)
            return new UpdateFriendResponse(result);
        
        try
        {
            // Resolve
            var resolved = PermissionResolver.Resolve(global, request.Permissions);
            var payload = new SyncPermissionsPayload(resolved);
            var message = new Message<SyncPermissionsPayload>(senderFriendCode, payload);
            await clients.Client(session.ConnectionId).SendAsync(HubMethod.SyncPermissions, message);
        }
        catch (Exception e)
        {
            logger.LogWarning("{Issuer} send action to {Target} failed, {Error}", senderFriendCode, request.TargetFriendCode, e.Message);
        }
        
        return new UpdateFriendResponse(result);
    }
}