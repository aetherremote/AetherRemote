using AetherRemoteCommon.Domain.Enums;
using MessagePack;

namespace AetherRemoteCommon.Network.Domain.Payloads;

[MessagePackObject]
public record BodySwapPayload(
    [property: Key(0)] CharacterAttributes SwapAttributes,
    [property: Key(1)] bool IncludeSelf,
    [property: Key(2)] string? LockCode
);
