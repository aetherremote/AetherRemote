using AetherRemoteClient.Handlers;
using AetherRemoteClient.Services;

namespace AetherRemoteClient.UI.Views.Settings;

public class SettingsViewUiController(
    ActionQueueService actionQueueService,
    SpiralService spiralService,
    PermanentTransformationHandler permanentTransformationHandler)
{
    /// <summary>
    ///     Updates safe mode, and clears all pending actions, spirals, etc...
    /// </summary>
    public void EnterSafeMode(bool safeMode)
    {
        // Save the configuration always
        Plugin.Configuration.Save();
        
        // Only proceed if safe mode is enabled
        if (safeMode is false)
            return;

        // Unlock permanent transformations
        permanentTransformationHandler.ForceClearPermanentTransformation();
        
        // Stop spirals
        spiralService.StopCurrentSpiral();
        
        // Clear action queue
        actionQueueService.Clear();
    }
}