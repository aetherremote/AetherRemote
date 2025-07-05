using MessagePack;

namespace AetherRemoteCommon.Domain.Network.Hypnosis;

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