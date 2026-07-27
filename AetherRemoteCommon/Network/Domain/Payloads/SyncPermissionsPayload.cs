using AetherRemoteCommon.Domain;
using MessagePack;

namespace AetherRemoteCommon.Network.Domain.Payloads;

[MessagePackObject]
public record SyncPermissionsPayload(
    [property: Key(0)] ResolvedPermissions PermissionsGrantedBySender
);
