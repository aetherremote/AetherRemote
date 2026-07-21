using AetherRemoteCommon.Domain;
using MessagePack;

namespace AetherRemoteCommon.Network.Domain.Payloads;

[MessagePackObject]
public record HypnosisPayload(
    [property: Key(0)] HypnosisData Data
);
