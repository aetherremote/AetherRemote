using MessagePack;

namespace AetherRemoteCommon.V2.Network.Commands;

[MessagePackObject]
public record AddFriendRequest(
    [property: Key(0)] string TargetFriendCode
);

