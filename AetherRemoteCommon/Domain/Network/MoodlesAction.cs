using MessagePack;

namespace AetherRemoteCommon.Domain.Network;

[MessagePackObject(keyAsPropertyName: true)]
public record MoodlesAction : BaseAction
{
    public string Moodle { get; set; } = string.Empty;
}