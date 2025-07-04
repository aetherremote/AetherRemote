using AetherRemoteCommon.Domain;
using AetherRemoteCommon.Domain.Network;
using MessagePack;

namespace AetherRemoteCommon.V2.Domain.Network.Hypnosis;

[MessagePackObject(keyAsPropertyName: true)]
public record HypnosisRequest : ActionRequest
{
    public SpiralInfo Spiral { get; set; } = new();

    public HypnosisRequest()
    {
    }

    public HypnosisRequest(List<string> targets, SpiralInfo spiral)
    {
        TargetFriendCodes =  targets;
        Spiral = spiral;
    }
}