using AetherRemoteCommon.Domain.Enums;
using MessagePack;

namespace AetherRemoteCommon.Network.Domain.Payloads;

[MessagePackObject]
public record TransformationPayload(
    [property: Key(0)] string GlamourerData,
    [property: Key(1)] GlamourerApplyFlags GlamourerApplyType,
    [property: Key(2)] string? LockCode
);
