using MessagePack;

namespace AetherRemoteCommon.Domain.Network;

[MessagePackObject(keyAsPropertyName: true)]
public record TwinningRequest : BaseRequest
{
    public CharacterIdentity Identity { get; set; } = new();
    public bool SwapMods { get; set; }
}