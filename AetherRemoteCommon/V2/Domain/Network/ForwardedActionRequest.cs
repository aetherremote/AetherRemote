using MessagePack;

namespace AetherRemoteCommon.V2.Domain.Network;

[MessagePackObject(true)]
public record ForwardedActionRequest
{
    public string SenderFriendCode { get; set; } = string.Empty;
}