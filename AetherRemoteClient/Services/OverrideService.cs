using AetherRemoteCommon.Domain;
using AetherRemoteCommon.Domain.Enums;

namespace AetherRemoteClient.Services;

/// <summary>
///     Provides access to the current overrides
/// </summary>
public class OverrideService
{
    /// <summary>
    ///     The current active overrides
    /// </summary>
    public readonly UserPermissions Overrides = Plugin.Configuration.TemporaryOverrides;

    /// <summary>
    ///     Is there a current override for a <see cref="PrimaryPermissions"/>?
    /// </summary>
    public bool HasActiveOverride(PrimaryPermissions permissions)
    {
        return (Overrides.Primary & permissions) == permissions;
    }

    /// <summary>
    ///     Is there a current override for a <see cref="LinkshellPermissions"/>?
    /// </summary>
    public bool HasActiveOverride(LinkshellPermissions permissions)
    {
        return (Overrides.Linkshell & permissions) == permissions;
    }
    
    /// <summary>
    ///     Save the current overrides
    /// </summary>
    public void Save()
    {
        Plugin.Configuration.TemporaryOverrides = Overrides;
        Plugin.Configuration.Save();
    }
}