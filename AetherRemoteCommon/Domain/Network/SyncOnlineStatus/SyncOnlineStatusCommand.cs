using AetherRemoteCommon.Domain.Enums;
using MessagePack;

namespace AetherRemoteCommon.Domain.Network.SyncOnlineStatus;

[MessagePackObject]
public record SyncOnlineStatusCommand(
    string SenderFriendCode,
    [property: Key(1)] FriendOnlineStatus Status,
    [property: Key(2)] UserPermissions? Permissions
) : ActionCommand(SenderFriendCode);