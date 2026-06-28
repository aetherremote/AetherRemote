using System.Threading.Tasks;
using AetherRemoteClient.Domain.Enums;
using AetherRemoteClient.Infrastructure.Database;

namespace AetherRemoteClient.Services;

/// <summary>
///     Provides access to the global settings in the plugin
/// </summary>
public class GlobalSettingsService(DatabaseInfrastructure database)
{
    /// <summary> Should the plugin be in safe mode? </summary>
    public bool SafeMode { get; private set; }
    
    /// <summary> Should the plugin display information on the Dtr bar? </summary>
    public bool ShowOnDtrBar { get; private set; }
    
    /// <summary>
    ///     Load all the global settings
    /// </summary>
    public async Task<bool> LoadGlobalSettings()
    {
        if (await database.GetGlobalSettings().ConfigureAwait(false) is not { } globalSettings)
            return false;

        SafeMode = globalSettings.TryGetValue(GlobalSetting.SafeMode, out var safeMode) && bool.Parse(safeMode);
        SafeMode = globalSettings.TryGetValue(GlobalSetting.ShowOnDtrBar, out var showOnDtrBar) && bool.Parse(showOnDtrBar);
        
        return true;
    }
    
    /// <summary>
    ///     Set the value of SafeMode
    /// </summary>
    public async Task<bool> SetSafeMode(bool value)
    {
        if (await database.SetGlobalSetting(GlobalSetting.SafeMode, value.ToString()).ConfigureAwait(false) is false)
            return false;
        
        SafeMode = value;
        return true;
    }
    
    /// <summary>
    ///     Set the value of SafeMode
    /// </summary>
    public async Task<bool> SetShowOnDtrBar(bool value)
    {
        if (await database.SetGlobalSetting(GlobalSetting.ShowOnDtrBar, value.ToString()).ConfigureAwait(false) is false)
            return false;
        
        ShowOnDtrBar = value;
        return true;
    }
}