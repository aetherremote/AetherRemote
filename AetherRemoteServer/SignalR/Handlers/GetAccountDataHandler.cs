using AetherRemoteCommon;
using AetherRemoteCommon.Domain.Enums;
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
        
        var results = new List<FriendRelationship>();
        var permissions = await database.GetAllPermissions(friendCode);
        foreach (var permission in permissions)
        {
            var online = permission.PermissionsGrantedBy is null
                ? FriendOnlineStatus.Pending
                : connections.TryGetClient(permission.TargetFriendCode) is null
                    ? FriendOnlineStatus.Offline
                    : FriendOnlineStatus.Online;
            
            results.Add(new FriendRelationship(permission.TargetFriendCode, online, permission.PermissionsGrantedTo, permission.PermissionsGrantedBy));
        } 

        return new GetAccountDataResponse(friendCode, results);
    }
}