using MessagePack;

namespace AetherRemoteCommon.Domain.Network.Hypnosis;

[MessagePackObject(keyAsPropertyName: true)]
public record HypnosisForwardedRequest : ForwardedActionRequest
{
    public HypnosisData Data { get; set; } = new();
    
    public HypnosisForwardedRequest()
    {
    }

    public HypnosisForwardedRequest(string senderFriendCode, HypnosisData data)
    {
        SenderFriendCode =  senderFriendCode;
        Data = data;
    }
}