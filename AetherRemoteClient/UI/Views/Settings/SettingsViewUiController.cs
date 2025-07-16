using AetherRemoteClient.Managers;
using AetherRemoteClient.Services;

namespace AetherRemoteClient.UI.Views.Settings;

public class SettingsViewUiController(ActionQueueService actionQueueService, IdentityService identityService, SpiralService spiralService, PermanentTransformationManager permanentTransformationManager)
{
    public int MinimumSpiralSpeed = 0;
    public int MaximumSpiralSpeed = 100;

    /// <summary>
    ///     Updates safe mode, and clears all pending actions, spirals, etc...
    /// </summary>
    public void UpdateSafeMode(bool safeMode)
    {
        Plugin.Configuration.Save();
        if (safeMode is false)
            return;

        // Unlock permanent transformations
        permanentTransformationManager.ForceUnlock();
        
        // Stop spirals
        spiralService.StopCurrentSpiral();
        
        // Clear action queue
        actionQueueService.Clear();
    }
}