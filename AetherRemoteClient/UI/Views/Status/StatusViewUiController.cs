using System;
using AetherRemoteClient.Ipc;
using AetherRemoteClient.Managers;
using AetherRemoteClient.Services;
using ImGuiNET;

namespace AetherRemoteClient.UI.Views.Status;

public class StatusViewUiController(NetworkService networkService, IdentityService identityService, GlamourerIpc glamourer, PermanentTransformationManager permanentTransformationManager)
{
    public string UnlockPin = "";
    public uint UnlockPinParsed = 0;

    public static readonly unsafe ImGuiInputTextCallback DigitsOnlyCallbackPointer = DigitsOnlyCallback;
    private static unsafe int DigitsOnlyCallback(ImGuiInputTextCallbackData* data)
    {
        if (data->EventFlag is not ImGuiInputTextFlags.CallbackCharFilter)
            return 0;
        
        if (data->EventChar < '0' || data->EventChar > '9')
            return 1;

        return 0;
    }

    /// <summary>
    ///     Attempt to unlock the client's appearance
    /// </summary>
    public void Unlock() => permanentTransformationManager.Unlock(UnlockPinParsed);
    
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