using AetherRemoteCommon.Domain;
using AetherRemoteCommon.Domain.Network.GetAccountData;
using AetherRemoteServer.Domain.Interfaces;

namespace AetherRemoteServer.SignalR.Handlers;

/// <summary>
///     Handles the logic for fulfilling a <see cref="GetAccountDataRequest"/>
/// </summary>
public class GetAccountDataHandler(IConnectionsService connections, IDatabaseService database)
{
    /// <summary>
    ///     Handles the request
    /// </summary>
    public async Task<GetAccountDataResponse> Handle(string friendCode, GetAccountDataRequest request)
    {
        if (connections.TryGetClient(friendCode) is not { } client)
            return new GetAccountDataResponse(GetAccountDataEc.NotOnline);

        client.CharacterName = request.CharacterName;
        
        var friendPermissions = await database.GetPermissions(friendCode);
        var permissionsGrantedToOthers = friendPermissions.Permissions;
        var permissionsGrantedByOthers = new Dictionary<string, UserPermissions>();
        foreach (var friend in permissionsGrantedToOthers)
        {
            if (connections.TryGetClient(friend.Key) is null)
                continue;

            var friendsPermissions = await database.GetPermissions(friend.Key);
            if (friendsPermissions.Permissions.TryGetValue(friendCode, out var permissionsGranted))
                permissionsGrantedByOthers[friend.Key] = permissionsGranted;
        }

        return new GetAccountDataResponse(friendCode, permissionsGrantedToOthers, permissionsGrantedByOthers);
    }
}