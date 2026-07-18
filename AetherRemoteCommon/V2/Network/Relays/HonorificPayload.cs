using AetherRemoteCommon.Domain.Honorific;
using MessagePack;

namespace AetherRemoteCommon.V2.Network.Relays;

[MessagePackObject]
public record HonorificPayload(
    [property: Key(0)] HonorificDto Data
);
