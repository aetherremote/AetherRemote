using AetherRemoteCommon.Domain;
using AetherRemoteCommon.Domain.Enums.Permissions;

namespace AetherRemoteServer.Domain;

/// <summary>
///     A set of permissions granted to and granted by a friend code
/// </summary>
/// <param name="TargetFriendCode">The friend code these permissions pertain to</param>
/// <param name="PermissionsGrantedTo">What we've granted to the other person</param>
/// <param name="PermissionsGrantedBy">Granted to us by the other person</param>
public record RawPermissionsGrantedResolvedPermissionGiven(
    string TargetFriendCode,
    RawPermissions PermissionsGrantedTo,
    ResolvedPermissions? PermissionsGrantedBy
);