using AetherRemoteCommon.Network.Enums;
using MessagePack;

namespace AetherRemoteCommon.Network.Domain.Commands;

[MessagePackObject]
public record UpdateGlobalPermissionsResponse(
    [property: Key(0)] ResponseStatus Status
);