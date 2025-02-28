using MessagePack;

namespace AetherRemoteCommon.Domain.Network;

[MessagePackObject(keyAsPropertyName: true)]
public record BodySwapQueryRequest
{
    public string SenderFriendCode { get;set; } = string.Empty;
}