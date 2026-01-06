using AetherRemoteCommon.Domain.Enums;
using MessagePack;

namespace AetherRemoteCommon.Domain.Network.SyncOnlineStatus;

[MessagePackObject(keyAsPropertyName: true)]
public record SyncOnlineStatusForwardedRequest : ForwardedActionRequest
{
    public FriendOnlineStatus Status { get; set; }
    
    public UserPermissions? Permissions { get; set; }

    public SyncOnlineStatusForwardedRequest()
    {
    }

    public SyncOnlineStatusForwardedRequest(string sender, FriendOnlineStatus status, UserPermissions? permissions)
    {
        SenderFriendCode = sender;
        Status = status;
        Permissions = permissions;
    }
}