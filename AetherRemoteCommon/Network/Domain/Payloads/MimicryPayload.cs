using AetherRemoteCommon.Domain.Enums;
using MessagePack;

namespace AetherRemoteCommon.Network.Domain.Payloads;

[MessagePackObject]
public record MimicryPayload(
    [property: Key(0)] CharacterAttributes Attributes,
    [property: Key(1)] string? LockCode
);