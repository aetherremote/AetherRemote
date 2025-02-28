using MessagePack;

namespace AetherRemoteCommon.Domain.Network;

[MessagePackObject(keyAsPropertyName: true)]
public record BodySwapQueryResponse
{
    public CharacterIdentity? Identity { get;set; }
}