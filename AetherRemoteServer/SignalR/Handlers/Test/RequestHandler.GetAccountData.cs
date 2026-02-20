using AetherRemoteCommon.Domain;
using AetherRemoteCommon.Domain.Enums;
using AetherRemoteCommon.Domain.Network.GetAccountData;
using AetherRemoteServer.Domain;

namespace AetherRemoteServer.SignalR.Handlers.Test;

public partial class RequestHandler
{
    /// <summary>
    ///     Handles the logic for fulfilling a <see cref="GetAccountDataRequest"/>
    /// </summary>
    public async Task<GetAccountDataResponse> HandleGetAccountData(string friendCode, string connectionId, GetAccountDataRequest request)
    {
        var presence = new Presence(connectionId, request.CharacterName, request.CharacterWorld);
        _presenceService.Add(friendCode, presence);
        
        var results = new List<FriendDto>();
        var global = await _databaseService.GetGlobalPermissions(friendCode);
        var permissions = await _databaseService.GetAllPermissions(friendCode);
        foreach (var permission in permissions)
        {
            var online = permission.PermissionsGrantedBy is null
                ? FriendOnlineStatus.Pending
                : _presenceService.TryGet(permission.TargetFriendCode) is null
                    ? FriendOnlineStatus.Offline
                    : FriendOnlineStatus.Online;
            
            results.Add(new FriendDto(permission.TargetFriendCode, online, permission.PermissionsGrantedTo, permission.PermissionsGrantedBy));
        } 

        return new GetAccountDataResponse(GetAccountDataEc.Success, friendCode, global, results);
    }
}