using AetherRemoteCommon.Domain;
using AetherRemoteCommon.Domain.Enums;
using AetherRemoteCommon.Domain.Enums.Permissions;
using AetherRemoteCommon.Network.Domain.Commands;
using AetherRemoteCommon.Network.Enums.ErrorCodes;
using AetherRemoteServer.Infrastructure.Database;
using AetherRemoteServer.Services;

namespace AetherRemoteServer.SignalR.Handlers;

public class InitializeSessionHandler(
    DatabaseInfrastructure databaseInfrastructure, 
    SessionService sessionService)
{
    private static readonly ResolvedPermissions EmptyPermissions = new(PrimaryPermissions.None, SpeakPermissions.None, ElevatedPermissions.None);
    
    public async Task<InitializeSessionResponse> Initialize(string senderFriendCode, string connectionId, InitializeSessionRequest request)
    {
        if (sessionService.StartSession(senderFriendCode, connectionId, request.CharacterName, request.CharacterWorld) is false)
            return new InitializeSessionResponse(GetAccountDataEc.AlreadyLoggedIn, string.Empty, EmptyPermissions, []);
        
        var senderGlobalPermissions = await databaseInfrastructure.GetGlobalPermissions(senderFriendCode);
        var senderAllPermissionPairs = await databaseInfrastructure.GetAllPermissions(senderFriendCode);
        
        var friends = new List<FriendDto>();
        foreach (var permissionPair in senderAllPermissionPairs)
        {
            var online = permissionPair.PermissionsGrantedBy is null
                ? FriendOnlineStatus.Pending
                : sessionService.IsOnline(permissionPair.TargetFriendCode)
                    ? FriendOnlineStatus.Online
                    : FriendOnlineStatus.Offline;
            
            friends.Add(new FriendDto(permissionPair.TargetFriendCode, online, permissionPair.PermissionsGrantedTo, permissionPair.PermissionsGrantedBy));
        }

        return new InitializeSessionResponse(GetAccountDataEc.Success, senderFriendCode, senderGlobalPermissions, friends);
    }
}