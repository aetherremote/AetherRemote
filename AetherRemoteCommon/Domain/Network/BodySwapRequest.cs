using AetherRemoteCommon.Domain.Enums;
using MessagePack;

namespace AetherRemoteCommon.Domain.Network;

[MessagePackObject(keyAsPropertyName: true)]
public record BodySwapRequest : BaseRequest
{
    /// <summary>
    ///     Set this if including self in swap
    /// </summary>
    public CharacterIdentity? Identity { get; set; }
    
    /// <summary>
    ///     Attributes to swap on a character
    /// </summary>
    public CharacterAttributes SwapAttributes { get; set; }
}