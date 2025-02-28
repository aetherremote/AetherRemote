using MessagePack;

namespace AetherRemoteCommon.Domain.Network;

[MessagePackObject(keyAsPropertyName: true)]
public record UpdateFriendRequest
{
    public string TargetFriendCode { get; set; } = string.Empty;
    public UserPermissions Permissions { get; set; } = new();
}