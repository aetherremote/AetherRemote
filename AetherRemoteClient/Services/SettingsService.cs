using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading.Tasks;
using AetherRemoteClient.Infrastructure.Database;

namespace AetherRemoteClient.Services;

public class SettingsService(DatabaseInfrastructure database)
{
    private Dictionary<string, string> _settings = [];
    private bool _dirty;
    
    public bool AutoLogin => _settings.TryGetValue(SettingKey.AutoLogin, out var value) && value switch
    {
        SettingValue.True => true,
        _ => false
    };

    public bool SafeMode => _settings.TryGetValue(SettingKey.SafeMode, out var value) && value switch
    {
        SettingValue.True => true,
        _ => false
    };
    
    public bool ShowDtrBar => _settings.TryGetValue(SettingKey.ShowDtrBar, out var value) && value switch
    {
        SettingValue.True => true,
        _ => false
    };
    
    public ImmutableDictionary<string, string> Settings
    {
        get
        {
            if (_dirty is false)
                return field;

            field = _settings.ToImmutableDictionary();
            _dirty = false;

            return field;
        }
    } = [];
    
    public async Task<bool> LoadSettings(long secretId)
    {
        if (await database.GetSettingsForSecretId(secretId).ConfigureAwait(false) is not { } settings)
            return false;
        
        _settings = settings;
        
        if (_settings.Count is 0)
        {
            if (await SetSetting(secretId, SettingKey.AutoLogin, SettingValue.False).ConfigureAwait(false) is false) return false;
            if (await SetSetting(secretId, SettingKey.SafeMode, SettingValue.False).ConfigureAwait(false) is false) return false;
            if (await SetSetting(secretId, SettingKey.ShowDtrBar, SettingValue.False).ConfigureAwait(false) is false) return false;
        }
        
        _dirty = true;
        return true;
    }

    /// <summary>
    ///     Set the value of a setting
    /// </summary>
    /// <param name="secretId">Which secret id should this setting apply to</param>
    /// <param name="key">The name of the setting of which are in the <see cref="SettingKey"/> class</param>
    /// <param name="value">The value of the setting of which are in the <see cref="SettingValue"/> class</param>
    /// <returns></returns>
    public async Task<bool> SetSetting(long secretId, string key, string value)
    {
        if (await database.SetSetting(secretId, key, value).ConfigureAwait(false) is false)
            return false;

        _settings[key] = value;
        _dirty = true;
        return true;
    }

    public static class SettingKey
    {
        public const string SafeMode = "SafeMode";
        public const string AutoLogin = "AutoLogin";
        public const string ShowDtrBar = "ShowDtrBar";
    }

    public static class SettingValue
    {
        public const string True = "true";
        public const string False = "false";
    }
}