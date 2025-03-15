using AetherRemoteCommon.Domain.Enums;
using MessagePack;

namespace AetherRemoteCommon.Domain.Network;

[MessagePackObject(keyAsPropertyName: true)]
public record TwinningRequest : BaseRequest
{
    public CharacterIdentity Identity { get; set; } = new();
   
    public CharacterAttributes SwapAttributes { get; set; }
}