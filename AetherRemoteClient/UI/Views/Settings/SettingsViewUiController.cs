using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AetherRemoteClient.Managers;
using AetherRemoteClient.Services;
using AetherRemoteClient.Utils;

namespace AetherRemoteClient.UI.Views.Settings;

public class SettingsViewUiController(
    ActionQueueService actionQueueService, 
    CharacterConfigurationService characterConfigurationService,
    GlobalSettingsService globalSettingsService,
    SecretsService secretsService, 
    SettingsService settingsService,
    DtrManager dtrManager,
    HypnosisManager hypnosisManager)
{
    // For use with maintaining an accurate secret usage map
    private static readonly TimeSpan OneSecond = TimeSpan.FromSeconds(1);
    private DateTime _lastRefreshed = DateTime.UtcNow;
    
    /// <summary>
    ///     Dictionary containing a map of secret id to character usage
    /// </summary>
    public readonly Dictionary<long, int> SecretUsageCharacterCount = [];
    
    public async Task AddSecret(string secretName, string secretValue)
    {
        if (await secretsService.AddSecret(secretName, secretValue).ConfigureAwait(false))
        {
            NotificationHelper.Success("Secret Added!", string.Empty);
        }
        else
        {
            NotificationHelper.Warning("Unable to Add Secret", "Make sure the name and secret are unique");
        }
    }

    public async Task RemoveSecret(long secretId)
    {
        if (await secretsService.RemoveSecret(secretId).ConfigureAwait(false))
        {
            NotificationHelper.Success("Secret Removed!", string.Empty);
        }
        else
        {
            NotificationHelper.Warning("Unable to Remove Secret", "See more details in the developer console by typing /xllog");
        }
    }

    public async Task SetAutoLogin(bool autoLogin)
    {
        if (characterConfigurationService.Current?.SecretId is not { } secretId) return;
        await settingsService.SetAutoLogin(secretId, autoLogin).ConfigureAwait(false);
    }

    public async Task SetShowDtrBar(bool showDtrBar)
    {
        if (await globalSettingsService.SetShowOnDtrBar(showDtrBar).ConfigureAwait(false) is false)
            return;

        if (showDtrBar)
            dtrManager.EnableDtrBar();
        else
            dtrManager.DisableDtrBar();
    }

    public async Task SetSafeMode(bool safeMode)
    {
        if (await globalSettingsService.SetSafeMode(safeMode).ConfigureAwait(false) is false)
            return;

        // When we enter safe mode, we want to disable a lot of things, so turning it off means we can exit early
        if (safeMode is false)
            return;
        
        // These are the things we want to turn off, since they are still potentially 'active' even if incoming commands are blocked
        hypnosisManager.Wake();
        actionQueueService.Clear();
    }
    
    /// <summary>
    ///     Refreshed the cached count of secrets in use
    /// </summary>
    /// <remarks>The primary intent is to prevent the database from calling this every frame</remarks>
    public async Task ShouldRefreshCharacterSecretUsage()
    {
        var now = DateTime.UtcNow;
        var last = _lastRefreshed;
        _lastRefreshed = now;
        
        if (now - last < OneSecond)
            return;
        
        Plugin.Log.Verbose("[SettingsViewUiController.RefreshSecretUsage] Refreshing Secret Usage...");

        SecretUsageCharacterCount.Clear();
        foreach (var secret in secretsService.Secrets)
            SecretUsageCharacterCount.Add(secret.Key, await secretsService.CountUsage(secret.Key).ConfigureAwait(false));
    }
}