using MessagePack;

namespace AetherRemoteCommon.Domain.Network.UpdateGlobalPermissions;

[MessagePackObject]
public record UpdateGlobalPermissionsRequest(
    [property: Key(0)] ResolvedPermissions Permissions
);