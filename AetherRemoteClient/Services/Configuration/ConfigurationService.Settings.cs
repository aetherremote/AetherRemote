using System.Threading.Tasks;
using AetherRemoteClient.Domain.Enums;

namespace AetherRemoteClient.Services.Configuration;

public partial class ConfigurationService
{
    /// <summary> Should the plugin be in safe mode? </summary>
    public bool SafeMode { get; private set; }
    
    /// <summary> Should the plugin display information on the Dtr bar? </summary>
    public bool ShowOnDtrBar { get; private set; }
    
    /// <summary>
    ///     Sets the value of a particular settings to a value
    /// </summary>
    public async Task<bool> SetSetting(Settings setting, bool value)
    {
        if (await databaseInfrastructure.SetSetting(setting, value.ToString()).ConfigureAwait(false) is false)
            return false;

        // This is not very scalable but that is probably okay for now
        switch (setting)
        {
            case Settings.SafeMode:
                SafeMode = value;
                return true;
            
            case Settings.ShowOnDtrBar:
                ShowOnDtrBar = value;
                return true;
            
            default:
                Plugin.Log.Warning($"[ConfigurationService.SetSafeMode] Unknown setting {setting}");
                return false;
        }
    }
    
    /// <summary>
    ///     Load all the global settings
    /// </summary>
    private async Task<bool> LoadSettings()
    {
        if (await databaseInfrastructure.GetSettings().ConfigureAwait(false) is not { } globalSettings)
            return false;

        SafeMode = globalSettings.TryGetValue(Settings.SafeMode, out var safeMode) && bool.Parse(safeMode);
        ShowOnDtrBar = globalSettings.TryGetValue(Settings.ShowOnDtrBar, out var showOnDtrBar) && bool.Parse(showOnDtrBar);
        return true;
    }
}