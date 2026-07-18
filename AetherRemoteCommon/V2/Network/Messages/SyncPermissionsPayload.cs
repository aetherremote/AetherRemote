using AetherRemoteCommon.Domain;
using MessagePack;

namespace AetherRemoteCommon.V2.Network.Messages;

[MessagePackObject]
public record SyncPermissionsPayload(
    [property: Key(0)] ResolvedPermissions PermissionsGrantedBySender
);
