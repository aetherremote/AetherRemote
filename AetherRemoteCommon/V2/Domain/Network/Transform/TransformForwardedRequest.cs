using AetherRemoteCommon.Domain.Enums;
using MessagePack;

namespace AetherRemoteCommon.V2.Domain.Network.Transform;

[MessagePackObject(keyAsPropertyName: true)]
public record TransformForwardedRequest : ForwardedActionRequest
{
    public string GlamourerData { get; set; } = string.Empty;
    public GlamourerApplyFlags GlamourerApplyType { get; set; }

    public TransformForwardedRequest()
    {
    }
    
    public TransformForwardedRequest(string sender, string data, GlamourerApplyFlags flags)
    {
        SenderFriendCode = sender;
        GlamourerData = data;
        GlamourerApplyType = flags;
    }
}