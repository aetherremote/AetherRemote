using AetherRemoteCommon.Domain;
using AetherRemoteCommon.Domain.Enums;
using AetherRemoteCommon.Domain.Enums.Permissions;
using AetherRemoteCommon.Network.Domain.Commands;
using AetherRemoteCommon.Network.Enums.ErrorCodes;
using AetherRemoteServer.Infrastructure.Database;
using AetherRemoteServer.Services;

namespace AetherRemoteServer.SignalR.Handlers;

public class GetAccountDataHandler(
    DatabaseInfrastructure databaseInfrastructure,
    PresenceService presenceService)
{
    private static readonly ResolvedPermissions EmptyPermissions = new(PrimaryPermissions.None, SpeakPermissions.None, ElevatedPermissions.None);

    public async Task<GetAccountDataResponse> Execute(string senderFriendCode, string connectionId, GetAccountDataRequest request)
    {
        if (presenceService.TryGet(senderFriendCode) is not null)
            return new GetAccountDataResponse(GetAccountDataEc.AlreadyLoggedIn, string.Empty, EmptyPermissions, []);
        
        presenceService.Add(senderFriendCode, connectionId, request.SenderCharacterName, request.SenderCharacterWorld);
        
        var friends = new List<FriendDto>();
        var globalPermissions = await databaseInfrastructure.GetGlobalPermissions(senderFriendCode);
        var permissionsGrantedToFriends = await databaseInfrastructure.GetAllPermissions(senderFriendCode);
        foreach (var permission in permissionsGrantedToFriends)
        {
            var online = permission.PermissionsGrantedBy is null
                ? FriendOnlineStatus.Pending
                : presenceService.TryGet(permission.TargetFriendCode) is null
                    ? FriendOnlineStatus.Offline
                    : FriendOnlineStatus.Online;
            
            friends.Add(new FriendDto(permission.TargetFriendCode, online, permission.PermissionsGrantedTo, permission.PermissionsGrantedBy));
        }

        return new GetAccountDataResponse(GetAccountDataEc.Success, senderFriendCode, globalPermissions, friends);
    }
}