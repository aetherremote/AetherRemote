using AetherRemoteClient.Domain;

namespace AetherRemoteClient.Services;

/// <summary>
///     Keep track of what parts of your character are modified
/// </summary>
public class StatusService
{
    /// <summary>
    ///     Any Customize changes
    /// </summary>
    public AetherRemoteStatus? CustomizePlus { get; set; }
    
    /// <summary>
    ///     Any glamourer or penumbra changes
    /// </summary>
    public AetherRemoteStatus? GlamourerPenumbra { get; set; }
    
    /// <summary>
    ///     Any Honorific changes
    /// </summary>
    public AetherRemoteStatus? Honorific { get; set; }
    
    /// <summary>
    ///     Any hypnosis
    /// </summary>
    public AetherRemoteStatus? Hypnosis { get; set; }

    /// <summary>
    ///     Any possession
    /// </summary>
    public AetherRemoteStatus? Possession { get; set; }
}
