using MessagePack;

namespace AetherRemoteCommon.Domain.Network;

[MessagePackObject(keyAsPropertyName: true)]
public record HypnosisAction : BaseAction
{
    public SpiralInfo Spiral { get; set; } = new();
}