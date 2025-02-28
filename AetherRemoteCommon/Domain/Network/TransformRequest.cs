using AetherRemoteCommon.Domain.Enums;
using MessagePack;

namespace AetherRemoteCommon.Domain.Network;

[MessagePackObject(keyAsPropertyName: true)]
public record TransformRequest : BaseRequest
{
    public string GlamourerData { get; set; } = string.Empty;
    public GlamourerApplyFlag GlamourerApplyType { get; set; }
}