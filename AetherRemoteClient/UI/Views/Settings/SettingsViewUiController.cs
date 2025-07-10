using AetherRemoteClient.Services;

namespace AetherRemoteClient.UI.Views.Settings;

public class SettingsViewUiController(ActionQueueService actionQueueService, IdentityService identityService, PermanentLockService permanentLockService, SpiralService spiralService)
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
        permanentLockService.CurrentLock = null;
        Plugin.Configuration.PermanentTransformations.Remove(identityService.Character.FullName);
        Plugin.Configuration.Save();
        
        // Stop spirals
        spiralService.StopCurrentSpiral();
        
        // Clear action queue
        actionQueueService.Clear();
    }
}