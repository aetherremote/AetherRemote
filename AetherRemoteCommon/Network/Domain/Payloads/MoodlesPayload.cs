using AetherRemoteCommon.Domain.Moodles;
using MessagePack;

namespace AetherRemoteCommon.Network.Domain.Payloads;

[MessagePackObject]
public record MoodlesPayload(
    [property: Key(0)] MoodleInfo Info
);
