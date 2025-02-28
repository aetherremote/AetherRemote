using MessagePack;

namespace AetherRemoteCommon.Domain.Network;

[MessagePackObject(keyAsPropertyName: true)]
public record BodySwapRequest : BaseRequest
{
    /// <summary>
    ///     Should mods be swapped?
    /// </summary>
    public bool SwapMods { get; set; }
    
    /// <summary>
    ///     Set this if including self in swap
    /// </summary>
    public CharacterIdentity? Identity { get; set; }
}