using MessagePack;

namespace AetherRemoteCommon.Domain.Network;

[MessagePackObject(keyAsPropertyName: true)]
public record BodySwapResponse : BaseResponse
{
    public CharacterIdentity? Identity { get; set; }
}