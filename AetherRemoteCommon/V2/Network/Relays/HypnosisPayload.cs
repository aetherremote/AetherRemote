using AetherRemoteCommon.Domain;
using MessagePack;

namespace AetherRemoteCommon.V2.Network.Relays;

[MessagePackObject]
public record HypnosisPayload(
    [property: Key(0)] HypnosisData Data
);
