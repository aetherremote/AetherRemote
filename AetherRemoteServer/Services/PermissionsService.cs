using AetherRemoteCommon.Domain;
using AetherRemoteCommon.Util;
using AetherRemoteCommon.V2.Domain;
using DatabaseInfrastructure = AetherRemoteServer.Infrastructure.Database.DatabaseInfrastructure;

namespace AetherRemoteServer.Services;

public class PermissionsService(DatabaseInfrastructure databaseInfrastructure)
{
    public async Task<RoutedResponseStatus?> ValidatePermissions(string senderFriendCode, string targetFriendCode, ResolvedPermissions required)
    {
        if (await databaseInfrastructure.GetSinglePermissions(targetFriendCode, senderFriendCode) is not { } permissions)
            return RoutedResponseStatus.NotFriends;

        var global = await databaseInfrastructure.GetGlobalPermissions(targetFriendCode);
        var granted = PermissionResolver.Resolve(global, permissions);
        
        if ((granted.Primary & required.Primary) != required.Primary || (granted.Speak & required.Speak) != required.Speak || (granted.Elevated & required.Elevated) != required.Elevated)
            return RoutedResponseStatus.NotGrantedPermissions;

        return null;
    }
}