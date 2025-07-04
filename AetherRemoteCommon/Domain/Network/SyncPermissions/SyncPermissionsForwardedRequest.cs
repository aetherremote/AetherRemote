using AetherRemoteCommon.Domain;
using AetherRemoteCommon.Domain.Network;
using MessagePack;

namespace AetherRemoteCommon.V2.Domain.Network.SyncPermissions;

[MessagePackObject(keyAsPropertyName: true)]
public record SyncPermissionsForwardedRequest : ForwardedActionRequest
{
    /// <summary>
    ///     Permissions granted by the sender of this action
    /// </summary>
    public UserPermissions PermissionsGrantedBySender { get; set; } = new();

    public SyncPermissionsForwardedRequest()
    {
    }

    public SyncPermissionsForwardedRequest(string sender, UserPermissions permissionsGrantedBySender)
    {
        SenderFriendCode = sender;
        PermissionsGrantedBySender = permissionsGrantedBySender;
    }
}