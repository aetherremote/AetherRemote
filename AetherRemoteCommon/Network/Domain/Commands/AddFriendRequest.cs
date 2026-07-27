using MessagePack;

namespace AetherRemoteCommon.Network.Domain.Commands;

[MessagePackObject]
public record AddFriendRequest(
    [property: Key(0)] string TargetFriendCode
);
