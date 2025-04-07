using MessagePack;

namespace AetherRemoteCommon.Domain.Network;

[MessagePackObject(keyAsPropertyName: true)]
public record CustomizePlusRequest : BaseRequest
{
    public string Customize { get; set; } = string.Empty;
}