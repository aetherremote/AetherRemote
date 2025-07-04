using AetherRemoteCommon.Domain;
using AetherRemoteCommon.Domain.Network;
using MessagePack;

namespace AetherRemoteCommon.V2.Domain.Network.Hypnosis;

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