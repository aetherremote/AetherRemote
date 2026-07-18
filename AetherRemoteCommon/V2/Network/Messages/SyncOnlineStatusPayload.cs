using AetherRemoteCommon.Domain;
using AetherRemoteCommon.Domain.Enums;
using MessagePack;

namespace AetherRemoteCommon.V2.Network.Messages;

[MessagePackObject]
public record SyncOnlineStatusPayload(
    [property: Key(0)] FriendOnlineStatus Status,
    [property: Key(1)] ResolvedPermissions? Permissions
);
