using AetherRemoteCommon.Domain.Enums;
using MessagePack;

namespace AetherRemoteCommon.Domain.Network;

[MessagePackObject(keyAsPropertyName: true)]
public record BodySwapAction : BaseAction
{
    public CharacterIdentity Identity { get; set; } = new();
    public CharacterAttributes SwapAttributes { get; set; }
}