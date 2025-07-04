using MessagePack;

namespace AetherRemoteCommon.Domain.Network;

[MessagePackObject(true)]
public record ForwardedActionRequest
{
    public string SenderFriendCode { get; set; } = string.Empty;
}