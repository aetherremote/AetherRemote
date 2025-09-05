using MessagePack;

namespace AetherRemoteCommon.Domain.Network.Hypnosis;

[MessagePackObject(keyAsPropertyName: true)]
public record HypnosisRequest : ActionRequest
{
    public HypnosisData Data { get; set; } = new();
    
    public bool Stop { get; set; }

    public HypnosisRequest()
    {
    }

    public HypnosisRequest(List<string> targets, HypnosisData data, bool stop)
    {
        TargetFriendCodes =  targets;
        Data = data;
        Stop = stop;
    }
}