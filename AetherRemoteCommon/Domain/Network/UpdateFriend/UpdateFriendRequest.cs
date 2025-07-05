using MessagePack;

namespace AetherRemoteCommon.Domain.Network.UpdateFriend;

[MessagePackObject(keyAsPropertyName: true)]
public record UpdateFriendRequest
{
    public string TargetFriendCode { get; set; } = string.Empty;
    public UserPermissions Permissions { get; set; } = new();

    public UpdateFriendRequest()
    {
    }

    public UpdateFriendRequest(string target, UserPermissions permissions)
    {
        TargetFriendCode = target;
        Permissions = permissions;
    }
}