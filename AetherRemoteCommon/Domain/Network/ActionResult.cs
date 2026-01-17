using AetherRemoteCommon.Domain.Enums;
using MessagePack;

namespace AetherRemoteCommon.Domain.Network;

[MessagePackObject]
public record ActionResult<T>(
    [property: Key(0)] ActionResultEc Result,
    [property: Key(1)] T? Value
);