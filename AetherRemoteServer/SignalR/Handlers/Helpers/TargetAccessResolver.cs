using AetherRemoteCommon.Domain.Enums;
using AetherRemoteCommon.V2;
using AetherRemoteServer.Domain.Interfaces;

namespace AetherRemoteServer.SignalR.Handlers.Helpers;

/// <summary>
///     TODO
/// </summary>
public class TargetAccessResolver(IClientConnectionService connections, IDatabaseService database)
{
    /// <summary>
    ///     TODO
    /// </summary>
    public async Task<AetherRemoteAction<string>> TryGetAuthorizedConnectionAsync(string sender, string target, PrimaryPermissions primary)
    {
        if (connections.TryGetClient(target) is not { } connectedClient)
            return AetherRemoteActionBuilder.Fail<string>(AetherRemoteActionErrorCode.TargetOffline);
        
        var targetPermissions = await database.GetPermissions(target);
        if (targetPermissions.Permissions.TryGetValue(sender, out var permissionsGranted) is false)
            return AetherRemoteActionBuilder.Fail<string>(AetherRemoteActionErrorCode.TargetNotFriends);

        if (primary is PrimaryPermissions.None || (permissionsGranted.Primary & primary) == primary)
            return AetherRemoteActionBuilder.Ok(connectedClient.ConnectionId);
        
        return AetherRemoteActionBuilder.Fail<string>(AetherRemoteActionErrorCode.TargetHasNotGrantedSenderPermissions);
    }
    
    /// <summary>
    ///     TODO
    /// </summary>
    public async Task<AetherRemoteAction<string>> TryGetAuthorizedConnectionAsync(string sender, string target, LinkshellPermissions linkshell)
    {
        if (connections.TryGetClient(target) is not { } connectedClient)
            return AetherRemoteActionBuilder.Fail<string>(AetherRemoteActionErrorCode.TargetOffline);
        
        var targetPermissions = await database.GetPermissions(target);
        if (targetPermissions.Permissions.TryGetValue(sender, out var permissionsGranted) is false)
            return AetherRemoteActionBuilder.Fail<string>(AetherRemoteActionErrorCode.TargetNotFriends);

        if (linkshell is LinkshellPermissions.None || (permissionsGranted.Linkshell & linkshell) == linkshell)
            return AetherRemoteActionBuilder.Ok(connectedClient.ConnectionId);
        
        return AetherRemoteActionBuilder.Fail<string>(AetherRemoteActionErrorCode.TargetHasNotGrantedSenderPermissions);
    }
}