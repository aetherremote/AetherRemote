using MessagePack;

namespace AetherRemoteCommon.Domain.Network;

[MessagePackObject(keyAsPropertyName: true)]
public record RemoveFriendRequest
{
    public string TargetFriendCode { get; set; } = string.Empty;
}