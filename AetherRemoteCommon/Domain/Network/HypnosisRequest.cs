using MessagePack;

namespace AetherRemoteCommon.Domain.Network;

[MessagePackObject(keyAsPropertyName: true)]
public record HypnosisRequest : BaseRequest
{
    public SpiralInfo Spiral { get; set; } = new();
}