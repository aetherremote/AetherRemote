using MessagePack;

namespace AetherRemoteCommon.Domain.Network;

[MessagePackObject(keyAsPropertyName: true)]
public record CustomizePlusAction : BaseAction
{
    public string Customize { get; set; } = string.Empty;
}