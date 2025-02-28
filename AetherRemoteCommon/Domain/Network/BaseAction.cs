using MessagePack;

namespace AetherRemoteCommon.Domain.Network;

[MessagePackObject(keyAsPropertyName: true)]
public record BaseAction
{
    public string SenderFriendCode { get; set; } = string.Empty;
}