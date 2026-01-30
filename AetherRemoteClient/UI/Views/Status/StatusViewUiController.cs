using System;
using System.Threading.Tasks;
using AetherRemoteClient.Dependencies.CustomizePlus.Services;
using AetherRemoteClient.Dependencies.Glamourer.Services;
using AetherRemoteClient.Dependencies.Honorific.Services;
using AetherRemoteClient.Dependencies.Penumbra.Services;
using AetherRemoteClient.Handlers;
using AetherRemoteClient.Managers.Possession;
using AetherRemoteClient.Services;
using AetherRemoteClient.UI.Components.Input;
using AetherRemoteClient.Utils;

namespace AetherRemoteClient.UI.Views.Status;

public class StatusViewUiController(
    NetworkService networkService,
    IdentityService identityService,
    GlamourerService glamourer,
    CustomizePlusService customizePlus,
    HonorificService honorific,
    PenumbraService penumbra,
    PossessionManager possessionManager,
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
            if (await glamourer.RevertToAutomation(0).ConfigureAwait(false) is false)
                return;

            identityService.ClearAlterations();
        }
        catch (Exception e)
        {
            Plugin.Log.Warning($"[StatusViewUiController] Unable to reset identity, {e.Message}");
        }
    }

    public async void ResetHonorific()
    {
        try
        {
            await honorific.ClearCharacterTitle(0).ConfigureAwait(false);
        }
        catch (Exception)
        {
            // Ignored
        }
    }
    
    public async void ResetCollection()
    {
        try
        {
            var guid = await penumbra.GetCollection().ConfigureAwait(false);
            await penumbra.CallRemoveTemporaryMod(guid).ConfigureAwait(false);
        }
        catch (Exception)
        {
            // Ignored
        }
    }

    public async void ResetCustomize()
    {
        try
        {
            await customizePlus.DeleteTemporaryCustomizeAsync().ConfigureAwait(false);
        }
        catch (Exception)
        {
            // Ignored
        }
    }

    public async Task Unpossess()
    {
        if (await possessionManager.Expel(false).ConfigureAwait(false))
            NotificationHelper.Success("Unpossess Successful", string.Empty);
    }
}