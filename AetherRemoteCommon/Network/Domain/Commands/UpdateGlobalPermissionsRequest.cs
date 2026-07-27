using AetherRemoteCommon.Domain;
using MessagePack;

namespace AetherRemoteCommon.Network.Domain.Commands;

[MessagePackObject]
public record UpdateGlobalPermissionsRequest(
    [property: Key(0)] ResolvedPermissions Permissions
);
