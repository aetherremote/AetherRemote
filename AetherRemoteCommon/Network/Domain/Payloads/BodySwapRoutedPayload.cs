using AetherRemoteCommon.Domain.Enums;
using MessagePack;

namespace AetherRemoteCommon.Network.Domain.Payloads;

[MessagePackObject]
public record BodySwapRoutedPayload(
    [property: Key(0)] CharacterAttributes SwapAttributes,
    [property: Key(1)] string CharacterName,
    [property: Key(2)] string CharacterWorld,
    [property: Key(3)] string? LockCode = null
);
