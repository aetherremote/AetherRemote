using AetherRemoteCommon.Domain.Enums;
using MessagePack;

namespace AetherRemoteCommon.Network.Domain.Payloads;

[MessagePackObject]
public record SpeakPayload(
    [property: Key(1)] string Message,
    [property: Key(2)] ChatChannel ChatChannel,
    [property: Key(3)] string? Extra
);
