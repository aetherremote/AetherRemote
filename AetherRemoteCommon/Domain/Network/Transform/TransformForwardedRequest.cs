using AetherRemoteCommon.Domain.Enums;
using MessagePack;

namespace AetherRemoteCommon.Domain.Network.Transform;

[MessagePackObject(keyAsPropertyName: true)]
public record TransformForwardedRequest : ForwardedActionRequest
{
    public string GlamourerData { get; set; } = string.Empty;
    public GlamourerApplyFlags GlamourerApplyType { get; set; }

    /// <summary>
    ///     Set this code to include a lock on the transform request
    /// </summary>
    public uint? LockCode { get; set; }

    public TransformForwardedRequest()
    {
    }
    
    public TransformForwardedRequest(string sender, string data, GlamourerApplyFlags flags, uint? lockCode)
    {
        SenderFriendCode = sender;
        GlamourerData = data;
        GlamourerApplyType = flags;
        LockCode =  lockCode;
    }
}