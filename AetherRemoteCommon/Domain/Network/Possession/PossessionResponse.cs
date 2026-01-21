using MessagePack;

namespace AetherRemoteCommon.Domain.Network.Possession;

[MessagePackObject]
public record PossessionResponse(
    [property: Key(0)] PossessionResponseEc Response,
    [property: Key(1)] PossessionResultEc Result
);