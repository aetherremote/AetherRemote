using AetherRemoteCommon.Domain.Honorific;
using MessagePack;

namespace AetherRemoteCommon.Network.Domain.Payloads;

[MessagePackObject]
public record HonorificPayload(
    [property: Key(0)] HonorificDto Data
);
