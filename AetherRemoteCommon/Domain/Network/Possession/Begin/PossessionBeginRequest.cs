using MessagePack;

namespace AetherRemoteCommon.Domain.Network.Possession.Begin;

[MessagePackObject]
public record PossessionBeginRequest(
    [property: Key(0)] string TargetFriendCode,
    [property: Key(1)] uint MoveMode
);