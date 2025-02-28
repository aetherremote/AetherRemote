using MessagePack;

namespace AetherRemoteCommon.Domain.Network;

[MessagePackObject(keyAsPropertyName: true)]
public record SyncOnlineStatusRequest
{
    public string FriendCode { get; set; } = string.Empty;
    public bool Online { get; set; }
}