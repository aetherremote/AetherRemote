using AetherRemoteCommon.Domain.Enums;
using MessagePack;

namespace AetherRemoteCommon.Domain;

/// <summary>
/// 
/// </summary>
/// <param name="TargetFriendCode">The friend code this pertains to</param>
/// <param name="Status">The status of the friend (online, offline, pending, etc...)</param>
/// <param name="PermissionsGrantedTo">The raw permissions we have granted to this friend</param>
/// <param name="PermissionsGrantedBy">The resolved permissions they have granted to us, if they are our friend too</param>
[MessagePackObject]
public record FriendDto(
    [property: Key(0)] string TargetFriendCode,
    [property: Key(1)] FriendOnlineStatus Status,
    [property: Key(2)] RawPermissions PermissionsGrantedTo,
    [property: Key(3)] ResolvedPermissions? PermissionsGrantedBy
);