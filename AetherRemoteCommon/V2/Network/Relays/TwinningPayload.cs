using AetherRemoteCommon.Domain.Enums;
using MessagePack;

namespace AetherRemoteCommon.V2.Network.Relays;

[MessagePackObject]
public record TwinningPayload(
    [property: Key(0)] string CharacterName,
    [property: Key(1)] string CharacterWorld,
    [property: Key(2)] CharacterAttributes SwapAttributes,
    [property: Key(3)] string? LockCode
);
