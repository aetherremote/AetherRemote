using AetherRemoteCommon.Domain;
using AetherRemoteCommon.Domain.Network;
using AetherRemoteServer.Managers;
using AetherRemoteServer.Services;

namespace AetherRemoteServer.Handlers;

/// <summary>
///     Handles the logic for fulfilling a <see cref="GetAccountDataRequest"/>
/// </summary>
public class GetAccountDataHandler(DatabaseService databaseService, ConnectedClientsManager connectedClientsManager)
{
    /// <summary>
    ///     Handles the request
    /// </summary>
    public async Task<GetAccountDataResponse> Handle(string friendCode, GetAccountDataRequest request)
    {
        var permissionsGrantedToOthers = await databaseService.GetPermissions(friendCode);
        var permissionsGrantedByOthers = new Dictionary<string, UserPermissions>();
        foreach (var friend in permissionsGrantedToOthers.Permissions)
        {
            if (connectedClientsManager.ConnectedClients.ContainsKey(friend.Key) is false)
                continue;

            var friendsPermissions = await databaseService.GetPermissions(friend.Key);
            if (friendsPermissions.Permissions.TryGetValue(friendCode, out var permissionsGranted))
                permissionsGrantedByOthers[friend.Key] = permissionsGranted;
        }

        return new GetAccountDataResponse
        {
            Success = true,
            FriendCode = friendCode,
            PermissionsGrantedToOthers = permissionsGrantedToOthers.Permissions,
            PermissionsGrantedByOthers = permissionsGrantedByOthers
        };
    }
}