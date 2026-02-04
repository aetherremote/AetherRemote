using AetherRemoteCommon.Domain;
using AetherRemoteCommon.Domain.Enums;
using AetherRemoteCommon.Domain.Network.GetAccountData;
using AetherRemoteServer.Domain;
using AetherRemoteServer.Services;
using AetherRemoteServer.Services.Database;

namespace AetherRemoteServer.SignalR.Handlers;

/// <summary>
///     Handles the logic for fulfilling a <see cref="GetAccountDataRequest"/>
/// </summary>
public class GetAccountDataHandler(DatabaseService database, PresenceService presenceService)
{
    /// <summary>
    ///     Handles the request
    /// </summary>
    public async Task<GetAccountDataResponse> Handle(string friendCode, string connectionId, GetAccountDataRequest request)
    {
        var presence = new Presence(connectionId, request.CharacterName, request.CharacterWorld);
        presenceService.Add(friendCode, presence);
        
        var results = new List<FriendDto>();
        var global = await database.GetGlobalPermissions(friendCode);
        var permissions = await database.GetAllPermissions(friendCode);
        foreach (var permission in permissions)
        {
            var online = permission.PermissionsGrantedBy is null
                ? FriendOnlineStatus.Pending
                : presenceService.TryGet(permission.TargetFriendCode) is null
                    ? FriendOnlineStatus.Offline
                    : FriendOnlineStatus.Online;
            
            results.Add(new FriendDto(permission.TargetFriendCode, online, permission.PermissionsGrantedTo, permission.PermissionsGrantedBy));
        } 

        return new GetAccountDataResponse(GetAccountDataEc.Success, friendCode, global, results);
    }
}