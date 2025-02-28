using MessagePack;

namespace AetherRemoteCommon.Domain.Network;

[MessagePackObject(keyAsPropertyName: true)]
public record EmoteAction : BaseAction
{
    public string Emote { get; set; } = string.Empty;
    public bool DisplayLogMessage { get; set; }
}