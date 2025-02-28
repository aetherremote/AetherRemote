using MessagePack;

namespace AetherRemoteCommon.Domain.Network;

[MessagePackObject(keyAsPropertyName: true)]
public record BaseResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
}