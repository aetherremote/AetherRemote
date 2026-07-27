using AetherRemoteCommon.Domain;
using AetherRemoteCommon.Domain.Enums;
using MessagePack;

namespace AetherRemoteCommon.Network.Domain.Payloads;

[MessagePackObject]
public record SyncOnlineStatusPayload(
    [property: Key(0)] FriendOnlineStatus Status,
    [property: Key(1)] ResolvedPermissions? Permissions = null
);
