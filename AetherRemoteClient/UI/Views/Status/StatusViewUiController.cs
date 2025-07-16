using System;
using AetherRemoteClient.Ipc;
using AetherRemoteClient.Managers;
using AetherRemoteClient.Services;
using ImGuiNET;

namespace AetherRemoteClient.UI.Views.Status;

public class StatusViewUiController(NetworkService networkService, IdentityService identityService, GlamourerIpc glamourer, PermanentTransformationManager permanentTransformationManager)
{
    public string UnlockPin = "";
    
    /// <summary>
    ///     Attempt to unlock the client's appearance
    /// </summary>
    public void Unlock() => permanentTransformationManager.Unlock(UnlockPin);

    public void Purge() => permanentTransformationManager.Purge();
    
    public async void Disconnect()
    {
        try
        {
            await networkService.StopAsync().ConfigureAwait(false);
        }
        catch (Exception e)
        {
            Plugin.Log.Warning($"[StatusViewUiController] Unable to disconnect from the server, {e.Message}");
        }
    }

    public async void ResetIdentity()
    {
        try
        {
            if (await glamourer.RevertToAutomation().ConfigureAwait(false) is false)
                return;
            
            await identityService.SetIdentityToCurrentCharacter().ConfigureAwait(false);
        }
        catch (Exception e)
        {
            Plugin.Log.Warning($"[StatusViewUiController] Unable to reset identity, {e.Message}");
        }
    }
}