using AetherRemoteCommon.Domain.Enums;
using MessagePack;

namespace AetherRemoteCommon.Network.Domain.Payloads;

[MessagePackObject]
public record CustomizePlusPayload(
    [property: Key(0)] byte[] JsonBoneDataBytes,
    [property: Key(1)] CustomizeApplyMode ApplyMode
);
