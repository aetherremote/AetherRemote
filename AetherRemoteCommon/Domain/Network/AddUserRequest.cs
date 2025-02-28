using MessagePack;

namespace AetherRemoteCommon.Domain.Network;

[MessagePackObject(keyAsPropertyName: true)]
public record AddUserRequest
{
    public string FriendCode { get; set; } = string.Empty;
    public string Secret { get; set; } = string.Empty;
    public bool IsAdmin { get; set; } = false;
}