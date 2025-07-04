using AetherRemoteCommon.Domain;
using AetherRemoteCommon.Domain.Network;
using MessagePack;

namespace AetherRemoteCommon.V2.Domain.Network.SyncOnlineStatus;

[MessagePackObject(keyAsPropertyName: true)]
public record SyncOnlineStatusForwardedRequest : ForwardedActionRequest
{
    public bool Online { get; set; }
    
    /// <summary>
    ///     Null if Online is false, otherwise expected to be not null
    /// </summary>
    public UserPermissions? Permissions { get; set; }

    public SyncOnlineStatusForwardedRequest()
    {
    }

    public SyncOnlineStatusForwardedRequest(string sender, bool online, UserPermissions? permissions)
    {
        SenderFriendCode = sender;
        Online = online;
        Permissions = permissions;
    }
}