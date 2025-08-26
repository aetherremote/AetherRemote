using System;
using AetherRemoteClient.Handlers;
using AetherRemoteClient.Managers;
using AetherRemoteClient.Services;
using AetherRemoteClient.Services.Dependencies;
using AetherRemoteClient.UI.Components.Input;
using AetherRemoteCommon.Domain.Enums;
using Dalamud.Bindings.ImGui;
using Newtonsoft.Json.Linq;

namespace AetherRemoteClient.UI.Views.Status;

public class StatusViewUiController(
    GlamourerService g,
    NetworkService networkService,
    IdentityService identityService,
    GlamourerService glamourer,
    PermanentTransformationHandler permanentTransformationHandler)
{
    public readonly FourDigitInput PinInput = new("StatusInput");
    
    /// <summary>
    ///     Attempt to unlock the client's appearance
    /// </summary>
    public void Unlock() => permanentTransformationHandler.TryClearPermanentTransformation(PinInput.Value);
    
    /// <summary>
    ///     Button event to trigger a server disconnect
    /// </summary>
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

    /// <summary>
    ///     Button event to trigger an identity reset
    /// </summary>
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