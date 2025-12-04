using MessagePack;

namespace AetherRemoteCommon.Domain.Network.Hypnosis;

[MessagePackObject(keyAsPropertyName: true)]
public record HypnosisRequest : ActionRequest
{
    public HypnosisData Data { get; set; } = new();

    public HypnosisRequest()
    {
    }

    public HypnosisRequest(List<string> targets, HypnosisData data)
    {
        TargetFriendCodes =  targets;
        Data = data;
    }
}