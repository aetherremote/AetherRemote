using AetherRemoteCommon.Domain;
using AetherRemoteCommon.Domain.Network;
using AetherRemoteServer.Domain.Interfaces;

namespace AetherRemoteServer.SignalR.Handlers;

/// <summary>
///     Handles the logic for fulfilling a <see cref="GetAccountDataRequest"/>
/// </summary>
public class GetAccountDataHandler(IClientConnectionService connections, IDatabaseService database)
{
    /// <summary>
    ///     Handles the request
    /// </summary>
    public async Task<GetAccountDataResponse> Handle(string friendCode, GetAccountDataRequest request)
    {
        var permissionsGrantedToOthers = await database.GetPermissions(friendCode);
        var permissionsGrantedByOthers = new Dictionary<string, UserPermissions>();
        foreach (var friend in permissionsGrantedToOthers.Permissions)
        {
            if (connections.TryGetClient(friend.Key) is null)
                continue;

            var friendsPermissions = await database.GetPermissions(friend.Key);
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