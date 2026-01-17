using AetherRemoteCommon.Domain.Enums;
using MessagePack;

namespace AetherRemoteCommon.Domain.Network;

[MessagePackObject]
public record ActionResponse(
    [property: Key(0)] ActionResponseEc Result,
    [property: Key(1)] Dictionary<string, ActionResultEc> Results
);