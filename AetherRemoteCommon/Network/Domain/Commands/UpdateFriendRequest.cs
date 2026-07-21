using AetherRemoteCommon.Domain;
using MessagePack;

namespace AetherRemoteCommon.Network.Domain.Commands;

[MessagePackObject]
public record UpdateFriendRequest(
    [property: Key(0)] string TargetFriendCode,
    [property: Key(1)] RawPermissions Permissions
);
