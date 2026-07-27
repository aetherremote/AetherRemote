using MessagePack;

namespace AetherRemoteCommon.Network.Domain.Commands;

[MessagePackObject]
public record RemoveFriendRequest(
    [property: Key(0)] string TargetFriendCode
);
