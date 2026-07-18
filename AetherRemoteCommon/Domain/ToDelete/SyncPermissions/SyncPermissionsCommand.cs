using MessagePack;

namespace AetherRemoteCommon.Domain.Network.SyncPermissions;

[MessagePackObject]
public record SyncPermissionsCommand(
    string SenderFriendCode,
    [property: Key(1)] ResolvedPermissions PermissionsGrantedBySender
) : ActionCommand(SenderFriendCode);