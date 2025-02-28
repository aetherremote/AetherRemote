using MessagePack;

namespace AetherRemoteCommon.Domain.Network;

[MessagePackObject(keyAsPropertyName: true)]
public record BodySwapAction : BaseAction
{
    public CharacterIdentity Identity { get; set; } = new();
    public bool SwapMods { get; set; }
}