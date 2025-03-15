using AetherRemoteCommon.Domain.Enums;
using MessagePack;

namespace AetherRemoteCommon.Domain.Network;

[MessagePackObject(keyAsPropertyName: true)]
public record TwinningAction : BaseAction
{
    public CharacterIdentity Identity { get; set; } = new();
    public CharacterAttributes SwapAttributes { get; set; }
}