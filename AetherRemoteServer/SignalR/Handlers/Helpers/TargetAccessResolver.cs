using AetherRemoteCommon.Domain.Enums;
using AetherRemoteCommon.V2;
using AetherRemoteCommon.V2.Domain;
using AetherRemoteCommon.V2.Domain.Enum;
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
    public async Task<ActionResult<string>> TryGetAuthorizedConnectionAsync(string sender, string target, PrimaryPermissions primary)
    {
        if (connections.TryGetClient(target) is not { } connectedClient)
            return ActionResultBuilder.Fail<string>(ActionResultEc.TargetOffline);
        
        var targetPermissions = await database.GetPermissions(target);
        if (targetPermissions.Permissions.TryGetValue(sender, out var permissionsGranted) is false)
            return ActionResultBuilder.Fail<string>(ActionResultEc.TargetNotFriends);

        if (primary is PrimaryPermissions.None || (permissionsGranted.Primary & primary) == primary)
            return ActionResultBuilder.Ok(connectedClient.ConnectionId);
        
        return ActionResultBuilder.Fail<string>(ActionResultEc.TargetHasNotGrantedSenderPermissions);
    }
    
    /// <summary>
    ///     TODO
    /// </summary>
    public async Task<ActionResult<string>> TryGetAuthorizedConnectionAsync(string sender, string target, LinkshellPermissions linkshell)
    {
        if (connections.TryGetClient(target) is not { } connectedClient)
            return ActionResultBuilder.Fail<string>(ActionResultEc.TargetOffline);
        
        var targetPermissions = await database.GetPermissions(target);
        if (targetPermissions.Permissions.TryGetValue(sender, out var permissionsGranted) is false)
            return ActionResultBuilder.Fail<string>(ActionResultEc.TargetNotFriends);

        if (linkshell is LinkshellPermissions.None || (permissionsGranted.Linkshell & linkshell) == linkshell)
            return ActionResultBuilder.Ok(connectedClient.ConnectionId);
        
        return ActionResultBuilder.Fail<string>(ActionResultEc.TargetHasNotGrantedSenderPermissions);
    }
}