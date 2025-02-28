using MessagePack;

namespace AetherRemoteCommon.Domain.Network;

[MessagePackObject(keyAsPropertyName: true)]
public record SyncPermissionsAction : BaseAction
{
    /// <summary>
    ///     Permissions granted by the sender of this action
    /// </summary>
    public UserPermissions PermissionsGrantedBySender { get; set; } = new();
}