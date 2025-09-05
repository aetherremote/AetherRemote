using MessagePack;

namespace AetherRemoteCommon.Domain.Network.Hypnosis;

[MessagePackObject(keyAsPropertyName: true)]
public record HypnosisForwardedRequest : ForwardedActionRequest
{
    public HypnosisData Data { get; set; } = new();

    public bool Stop { get; set; }
    
    public HypnosisForwardedRequest()
    {
    }

    public HypnosisForwardedRequest(string senderFriendCode, HypnosisData data, bool stop)
    {
        SenderFriendCode =  senderFriendCode;
        Data = data;
        Stop = stop;
    }
}