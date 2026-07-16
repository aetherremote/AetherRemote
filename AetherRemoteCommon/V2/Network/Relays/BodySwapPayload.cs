using AetherRemoteCommon.Domain.Enums;
using MessagePack;

namespace AetherRemoteCommon.V2.Network.Relays;

[MessagePackObject]
public record BodySwapPayload(
    [property: Key(0)] CharacterAttributes SwapAttributes,
    [property: Key(1)] bool IncludeSelf,
    [property: Key(2)] string? LockCode
);