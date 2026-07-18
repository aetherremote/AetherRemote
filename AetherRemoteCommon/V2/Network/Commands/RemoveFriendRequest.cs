using MessagePack;

namespace AetherRemoteCommon.V2.Network.Commands;

[MessagePackObject]
public record RemoveFriendRequest(
    [property: Key(0)] string TargetFriendCode
);
