using MessagePack;

namespace AetherRemoteCommon.Domain.Network;

[MessagePackObject(keyAsPropertyName: true)]
public record SyncOnlineStatusAction : BaseAction
{
    public bool Online { get; set; }
    
    /// <summary>
    ///     Null if Online is false, otherwise expected to be not null
    /// </summary>
    public UserPermissions? Permissions { get; set; }
}