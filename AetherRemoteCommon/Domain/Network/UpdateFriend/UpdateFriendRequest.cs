using MessagePack;

namespace AetherRemoteCommon.Domain.Network.UpdateFriend;

[MessagePackObject]
public record UpdateFriendRequest(
    [property: Key(0)] string TargetFriendCode,
    [property: Key(1)] RawPermissions Permissions
);