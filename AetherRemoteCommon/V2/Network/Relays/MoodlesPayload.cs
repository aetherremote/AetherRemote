using AetherRemoteCommon.Domain.Moodles;
using MessagePack;

namespace AetherRemoteCommon.V2.Network.Relays;

[MessagePackObject]
public record MoodlesPayload(
    [property: Key(0)] MoodleInfo Info
);
