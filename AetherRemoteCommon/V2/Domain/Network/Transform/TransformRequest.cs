using AetherRemoteCommon.Domain.Enums;
using MessagePack;

namespace AetherRemoteCommon.V2.Domain.Network.Transform;

[MessagePackObject(keyAsPropertyName: true)]
public record TransformRequest : ActionRequest
{
    public string GlamourerData { get; set; } = string.Empty;
    public GlamourerApplyFlags GlamourerApplyType { get; set; }
    
    public TransformRequest()
    {
    }
    
    public TransformRequest(List<string> targets, string data, GlamourerApplyFlags flags)
    {
        TargetFriendCodes = targets;
        GlamourerData = data;
        GlamourerApplyType = flags;
    }
}