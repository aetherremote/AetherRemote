using AetherRemoteCommon.Domain.Enums;
using AetherRemoteCommon.Domain.Network;
using AetherRemoteCommon.Domain.Network.SyncPermissions;
using AetherRemoteCommon.Domain.Network.UpdateFriend;
using AetherRemoteCommon.Util;
using Microsoft.AspNetCore.SignalR;

namespace AetherRemoteServer.SignalR.Handlers;

public partial class RequestHandler
{
    public async Task<UpdateFriendResponse> HandleUpdateFriend(string friendCode, UpdateFriendRequest request, IHubCallerClients clients)
    {
        var databaseResult = await _databaseService.UpdatePermissions(friendCode, request.TargetFriendCode, request.Permissions);
        var result = databaseResult switch
        {
            DatabaseResultEc.Success => UpdateFriendEc.Success,
            DatabaseResultEc.NoOp => UpdateFriendEc.NoOp,
            _ => UpdateFriendEc.Unknown
        };
        
        if (_presenceService.TryGet(request.TargetFriendCode) is not { } connectedClient)
            return new UpdateFriendResponse(result);
        
        // TODO: Update failure state. This is not an expected state
        if (await _databaseService.GetGlobalPermissions(friendCode) is not { } global)
            return new UpdateFriendResponse(result);
        
        try
        {
            // Resolve
            var resolved = PermissionResolver.Resolve(global, request.Permissions);
            var sync = new SyncPermissionsCommand(friendCode, resolved);
            await clients.Client(connectedClient.ConnectionId).SendAsync(HubMethod.SyncPermissions, sync);
        }
        catch (Exception e)
        {
            _logger.LogWarning("{Issuer} send action to {Target} failed, {Error}", friendCode, request.TargetFriendCode, e.Message);
        }

        return new UpdateFriendResponse(result);
    }
}