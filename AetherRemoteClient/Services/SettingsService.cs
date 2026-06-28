using System.Threading.Tasks;
using AetherRemoteClient.Domain.Enums;
using AetherRemoteClient.Infrastructure.Database;

namespace AetherRemoteClient.Services;

/// <summary>
///     Provides access to an individual secret's settings
/// </summary>
public class SettingsService(DatabaseInfrastructure database)
{
    /// <summary> Should a character using this secret automatically attempt to log in </summary>
    public bool AutoLogin { get; private set; }
    
    /// <summary>
    ///     Load all the settings for a secret
    /// </summary>
    public async Task<bool> LoadSettings(long secretId)
    {
        if (await database.GetSettingsForSecretId(secretId).ConfigureAwait(false) is not { } settings)
            return false;

        AutoLogin = settings.TryGetValue(Setting.AutoLogin, out var autoLogin) && bool.Parse(autoLogin);

        return true;
    }
    
    /// <summary>
    ///     Set the value of AutoLogin for a secret
    /// </summary>
    public async Task<bool> SetAutoLogin(long secretId, bool value)
    {
        if (await database.SetSetting(secretId, Setting.AutoLogin, value.ToString()).ConfigureAwait(false) is false)
            return false;
        
        AutoLogin = value;
        return true;
    }
}