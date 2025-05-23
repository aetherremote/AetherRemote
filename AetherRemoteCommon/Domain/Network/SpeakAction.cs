using AetherRemoteCommon.Domain.Enums;
using MessagePack;

namespace AetherRemoteCommon.Domain.Network;

[MessagePackObject(keyAsPropertyName: true)]
public record SpeakAction : BaseAction
{
    public string Message { get; set; } = string.Empty;
    public ChatChannel ChatChannel { get; set; }
    public string? Extra  { get; set; }
}