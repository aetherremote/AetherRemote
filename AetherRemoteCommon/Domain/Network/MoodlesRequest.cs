using MessagePack;

namespace AetherRemoteCommon.Domain.Network;

[MessagePackObject(keyAsPropertyName: true)]
public record MoodlesRequest : BaseRequest
{
    public string Moodle { get; set; } = string.Empty;
}