using AetherRemoteClient.Managers;
using AetherRemoteClient.Services;

namespace AetherRemoteClient.UI.Views.Settings;

public class SettingsViewUiController(SpiralService spiralService, ActionQueueManager actionQueueManager)
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
        
        // If we're in safe mode, begin disabling all active things
        spiralService.StopCurrentSpiral();
        actionQueueManager.Clear();
    }
}