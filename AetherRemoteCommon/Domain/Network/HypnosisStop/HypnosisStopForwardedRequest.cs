using MessagePack;

namespace AetherRemoteCommon.Domain.Network.HypnosisStop;

[MessagePackObject(keyAsPropertyName: true)]
public record HypnosisStopForwardedRequest : ForwardedActionRequest
{
    public HypnosisStopForwardedRequest()
    {
    }

    public HypnosisStopForwardedRequest(string senderFriendCode)
    {
        SenderFriendCode = senderFriendCode;
    }
}