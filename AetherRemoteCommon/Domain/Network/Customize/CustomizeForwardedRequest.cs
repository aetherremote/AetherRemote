using AetherRemoteCommon.Domain.Network;
using MessagePack;

namespace AetherRemoteCommon.V2.Domain.Network.Customize;

[MessagePackObject(true)]
public record CustomizeForwardedRequest : ForwardedActionRequest
{
    public string Data { get; set; } = string.Empty;

    public CustomizeForwardedRequest()
    {
    }

    public CustomizeForwardedRequest(string sender, string data)
    {
        SenderFriendCode = sender;
        Data = data;
    }
}