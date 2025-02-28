using MessagePack;

namespace AetherRemoteCommon.Domain.Network;

[MessagePackObject(keyAsPropertyName: true)]
public record AddFriendRequest
{
    public string TargetFriendCode { get; set; } = string.Empty;
}