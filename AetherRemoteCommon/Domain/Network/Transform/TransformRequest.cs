using AetherRemoteCommon.Domain.Enums;
using MessagePack;

namespace AetherRemoteCommon.Domain.Network.Transform;

[MessagePackObject(keyAsPropertyName: true)]
public record TransformRequest : ActionRequest
{
    public string GlamourerData { get; set; } = string.Empty;
    public GlamourerApplyFlags GlamourerApplyType { get; set; }

    /// <summary>
    ///     Set this code to include a lock on the transform request
    /// </summary>
    public uint? LockCode { get; set; }
    
    public TransformRequest()
    {
    }
    
    public TransformRequest(List<string> targets, string data, GlamourerApplyFlags flags, uint? lockCode)
    {
        TargetFriendCodes = targets;
        GlamourerData = data;
        GlamourerApplyType = flags;
        LockCode = lockCode;
    }
}