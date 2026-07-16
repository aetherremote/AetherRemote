using AetherRemoteCommon.Domain.Enums;
using MessagePack;

namespace AetherRemoteCommon.V2.Network.Relays;

[MessagePackObject]
public record CustomizePlusPayload(
    [property: Key(0)] byte[] JsonBoneDataBytes,
    [property: Key(1)] CustomizeApplyMode ApplyMode
);
