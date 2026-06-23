using System;
using System.Threading.Tasks;
using AetherRemoteClient.Domain.Enums;
using AetherRemoteClient.Infrastructure.Database;

namespace AetherRemoteClient.Services;

/// <summary>
///     Provides access to the various settings in the plugin
/// </summary>
/// <remarks> All the settings must match those defined in <see cref="Setting"/> </remarks>
public class SettingsService(DatabaseInfrastructure database)
{
    public bool AutoLogin { get; private set; }
    public bool SafeMode { get; private set; }
    public bool ShowDtrBar { get; private set; }
    
    public async Task<bool> LoadSettings(long secretId)
    {
        if (await database.GetSettingsForSecretId(secretId).ConfigureAwait(false) is not { } settings)
            return false;

        AutoLogin = settings.TryGetValue(Setting.AutoLogin, out var autoLogin) && bool.Parse(autoLogin);
        SafeMode = settings.TryGetValue(Setting.SafeMode, out var safeMode) && bool.Parse(safeMode);
        ShowDtrBar = settings.TryGetValue(Setting.ShowDtrBar, out var showDtrBar) && bool.Parse(showDtrBar);

        return true;
    }

    public Task<bool> SetAutoLogin(long secretId, bool value) => SetSetting(secretId, Setting.AutoLogin, value, v => AutoLogin = v);
    public Task<bool> SetSafeMode(long secretId, bool value) => SetSetting(secretId, Setting.SafeMode, value, v => SafeMode = v);
    public Task<bool> SetShowDtrBar(long secretId, bool value) => SetSetting(secretId, Setting.ShowDtrBar, value, v => ShowDtrBar = v);
    
    // Trick learned from the reflection in C#
    private async Task<bool> SetSetting<T>(long secretId, Setting setting, T value, Action<T> setter)
    {
        if (value?.ToString() is not { } stringValue)
            return false;
        
        if (await database.SetSetting(secretId, setting, stringValue).ConfigureAwait(false) is false)
            return false;
        
        setter(value);
        return true;
    }
}