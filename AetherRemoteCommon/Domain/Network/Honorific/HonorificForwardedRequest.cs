using AetherRemoteCommon.Dependencies.Honorific.Domain;
using MessagePack;

namespace AetherRemoteCommon.Domain.Network.Honorific;

[MessagePackObject(keyAsPropertyName: true)]
public record HonorificForwardedRequest : ForwardedActionRequest
{
    public HonorificInfo Honorific { get; set; } = new();

    public HonorificForwardedRequest()
    {
    }

    public HonorificForwardedRequest(string senderFriendCode, HonorificInfo honorific)
    {
        SenderFriendCode = senderFriendCode;
        Honorific = honorific;
    }
}