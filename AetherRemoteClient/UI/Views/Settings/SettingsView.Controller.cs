using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AetherRemoteClient.Utils;

namespace AetherRemoteClient.UI.Views.Settings;

public partial class SettingsView
{
    // For use with maintaining an accurate secret usage map
    private static readonly TimeSpan OneSecond = TimeSpan.FromSeconds(1);
    private DateTime _lastRefreshed = DateTime.UtcNow;
    
    /// <summary>
    ///     Dictionary containing a map of secret id to character usage
    /// </summary>
    private readonly Dictionary<long, int> _secretUsageCharacterCount = [];

    private async Task AddSecret(string secretName, string secretValue)
    {
        if (await _secretsService.AddSecret(secretName, secretValue).ConfigureAwait(false))
        {
            NotificationHelper.Success("Secret Added!", string.Empty);
        }
        else
        {
            NotificationHelper.Warning("Unable to Add Secret", "Make sure the name and secret are unique");
        }
    }

    private async Task RemoveSecret(long secretId)
    {
        if (await _secretsService.RemoveSecret(secretId).ConfigureAwait(false))
        {
            NotificationHelper.Success("Secret Removed!", string.Empty);
        }
        else
        {
            NotificationHelper.Warning("Unable to Remove Secret", "See more details in the developer console by typing /xllog");
        }
    }

    private async Task SetAutoLogin(bool autoLogin)
    {
        if (_characterConfigurationService.Current?.SecretId is not { } secretId) return;
        await _settingsService.SetAutoLogin(secretId, autoLogin).ConfigureAwait(false);
    }

    private async Task SetShowDtrBar(bool showDtrBar)
    {
        if (await _globalSettingsService.SetShowOnDtrBar(showDtrBar).ConfigureAwait(false) is false)
            return;

        if (showDtrBar)
            _dtrManager.EnableDtrBar();
        else
            _dtrManager.DisableDtrBar();
    }

    private async Task SetSafeMode(bool safeMode)
    {
        if (await _globalSettingsService.SetSafeMode(safeMode).ConfigureAwait(false) is false)
            return;

        // When we enter safe mode, we want to disable a lot of things, so turning it off means we can exit early
        if (safeMode is false)
            return;
        
        // These are the things we want to turn off, since they are still potentially 'active' even if incoming commands are blocked
        _hypnosisManager.Wake();
        _actionQueueService.Clear();
    }
    
    /// <summary>
    ///     Refreshed the cached count of secrets in use
    /// </summary>
    /// <remarks>The primary intent is to prevent the database from calling this every frame</remarks>
    private async Task ShouldRefreshCharacterSecretUsage()
    {
        var now = DateTime.UtcNow;
        var last = _lastRefreshed;
        _lastRefreshed = now;
        
        if (now - last < OneSecond)
            return;
        
        Plugin.Log.Verbose("[SettingsViewUiController.RefreshSecretUsage] Refreshing Secret Usage...");

        _secretUsageCharacterCount.Clear();
        foreach (var secret in _secretsService.Secrets)
            _secretUsageCharacterCount.Add(secret.Key, await _secretsService.CountUsage(secret.Key).ConfigureAwait(false));
    }
}