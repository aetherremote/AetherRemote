using AetherRemoteCommon.Domain;
using MessagePack;

namespace AetherRemoteCommon.V2.Network.Commands;

[MessagePackObject]
public record UpdateGlobalPermissionsRequest(
    [property: Key(0)] ResolvedPermissions Permissions
);
