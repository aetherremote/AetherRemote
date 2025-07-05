using MessagePack;

namespace AetherRemoteCommon.Domain.Network.Hypnosis;

[MessagePackObject(keyAsPropertyName: true)]
public record HypnosisForwardedRequest : ForwardedActionRequest
{
    public SpiralInfo Spiral { get; set; } = new();
    
    public HypnosisForwardedRequest()
    {
    }

    public HypnosisForwardedRequest(string senderFriendCode, SpiralInfo spiral)
    {
        SenderFriendCode =  senderFriendCode;
        Spiral = spiral;
    }
}