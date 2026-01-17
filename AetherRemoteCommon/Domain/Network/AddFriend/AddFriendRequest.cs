using MessagePack;

namespace AetherRemoteCommon.Domain.Network.AddFriend;

[MessagePackObject]
public record AddFriendRequest(
    [property: Key(0)] string TargetFriendCode
);

