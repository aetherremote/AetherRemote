using System;
using AetherRemoteClient.Handlers;
using AetherRemoteClient.Managers;
using AetherRemoteClient.Services;

namespace AetherRemoteClient.UI.Views.Settings;

public class SettingsViewUiController(
    ActionQueueService actionQueueService,
    HypnosisManager hypnosisManager,
    PermanentTransformationHandler permanentTransformationHandler)
{
    /// <summary>
    ///     Updates safe mode, and clears all pending actions, spirals, etc...
    /// </summary>
    public async void EnterSafeMode(bool safeMode)
    {
        try
        {
            // Save the configuration always
            await Plugin.Configuration.Save().ConfigureAwait(false);
        
            // Only proceed if safe mode is enabled
            if (safeMode is false)
                return;

            // Unlock permanent transformations
            permanentTransformationHandler.ForceClearPermanentTransformation();
        
            // Stop spirals
            hypnosisManager.Wake();
        
            // Clear action queue
            actionQueueService.Clear();
        }
        catch (Exception e)
        {
            Plugin.Log.Error($"[SettingsViewUiController.EnterSafeMode] {e}");
        }
    }

    public async void SaveConfiguration()
    {
        try
        {
            if (Plugin.CharacterConfiguration is null)
                return;
            
            await Plugin.CharacterConfiguration.Save().ConfigureAwait(false);
        }
        catch (Exception e)
        {
            Plugin.Log.Error($"[SettingsViewUiController.SaveConfiguration] {e}");
        }
    }
}