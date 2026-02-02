using AetherRemoteCommon.Domain;
using AetherRemoteCommon.Domain.Enums;
using AetherRemoteCommon.Domain.Network;
using AetherRemoteCommon.Domain.Network.SyncPermissions;
using AetherRemoteCommon.Domain.Network.UpdateFriend;
using AetherRemoteServer.Domain.Interfaces;
using Microsoft.AspNetCore.SignalR;

namespace AetherRemoteServer.SignalR.Handlers;

public class UpdateFriendHandler(IPresenceService presenceService, IDatabaseService database, ILogger<UpdateFriendHandler> logger)
{
    public async Task<UpdateFriendResponse> Handle(string friendCode, UpdateFriendRequest request, IHubCallerClients clients)
    {
        var databaseResult = await database.UpdatePermissions(friendCode, request.TargetFriendCode, request.Permissions);
        var result = databaseResult switch
        {
            DatabaseResultEc.Success => UpdateFriendEc.Success,
            DatabaseResultEc.NoOp => UpdateFriendEc.NoOp,
            _ => UpdateFriendEc.Unknown
        };
        
        if (presenceService.TryGet(request.TargetFriendCode) is not { } connectedClient)
            return new UpdateFriendResponse(result);
        
        // TODO: Update failure state. This is not an expected state
        if (await database.GetGlobalPermissions(friendCode) is not { } global)
            return new UpdateFriendResponse(result);
        
        try
        {
            // Resolve
            var resolved = Resolve(global, request.Permissions);
            var sync = new SyncPermissionsCommand(friendCode, resolved);
            await clients.Client(connectedClient.ConnectionId).SendAsync(HubMethod.SyncPermissions, sync);
        }
        catch (Exception e)
        {
            logger.LogWarning("{Issuer} send action to {Target} failed, {Error}", friendCode, request.TargetFriendCode, e.Message);
        }

        return new UpdateFriendResponse(result);
    }

    private static ResolvedPermissions Resolve(ResolvedPermissions global, RawPermissions raw)
    {
        var effectivePrimary = (global.Primary | raw.PrimaryAllow) & ~raw.PrimaryDeny;
        var effectiveSpeak = (global.Speak | raw.SpeakAllow) & ~raw.SpeakDeny;
        var effectiveElevated = (global.Elevated | raw.ElevatedAllow) & ~raw.ElevatedDeny;
        return new ResolvedPermissions(effectivePrimary, effectiveSpeak, effectiveElevated);
    }
}