using MessagePack;

namespace AetherRemoteCommon.Domain.Network.Possession.Movement;

[MessagePackObject]
public record PossessionMovementRequest(
    [property: Key(0)] float Horizontal,
    [property: Key(1)] float Vertical,
    [property: Key(2)] float Turn,
    [property: Key(3)] byte Backwards
);