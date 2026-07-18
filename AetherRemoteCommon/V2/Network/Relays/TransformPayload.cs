using AetherRemoteCommon.Domain.Enums;
using MessagePack;

namespace AetherRemoteCommon.V2.Network.Relays;

[MessagePackObject]
public record TransformPayload(
    [property: Key(0)] string GlamourerData,
    [property: Key(1)] GlamourerApplyFlags GlamourerApplyType,
    [property: Key(2)] string? LockCode
);
