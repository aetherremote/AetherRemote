using System;
using AetherRemoteClient.Dependencies;
using AetherRemoteClient.Managers;
using AetherRemoteClient.Services;
using AetherRemoteClient.UI.Components.Input;
using AetherRemoteCommon.Domain.Enums;
using Dalamud.Bindings.ImGui;
using Newtonsoft.Json.Linq;

namespace AetherRemoteClient.UI.Views.Status;

public class StatusViewUiController(
    GlamourerDependency g,
    NetworkManager networkManager,
    IdentityService identityService,
    GlamourerDependency glamourer,
    PermanentTransformationManager permanentTransformationManager)
{
    public readonly FourDigitInput PinInput = new("StatusInput");
    
    /// <summary>
    ///     Attempt to unlock the client's appearance
    /// </summary>
    public void Unlock() => permanentTransformationManager.TryClearPermanentTransformation(PinInput.Value);
    // TODO: Make this async or handled properly
    
    public async void Disconnect()
    {
        try
        {
            await networkManager.StopAsync().ConfigureAwait(false);
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

            identityService.ClearAlterations();
        }
        catch (Exception e)
        {
            Plugin.Log.Warning($"[StatusViewUiController] Unable to reset identity, {e.Message}");
        }
    }
}