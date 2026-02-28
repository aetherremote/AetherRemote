using System.Threading.Tasks;
using AetherRemoteClient.Dependencies.CustomizePlus.Services;
using AetherRemoteClient.Dependencies.Glamourer.Services;
using AetherRemoteClient.Dependencies.Honorific.Services;
using AetherRemoteClient.Dependencies.Penumbra.Services;
using AetherRemoteClient.Managers;
using AetherRemoteClient.Managers.Possession;

namespace AetherRemoteClient.UI.Views.Status;

public class StatusViewUiController(
    CustomizePlusService customizePlusService,
    GlamourerService glamourerService,
    HonorificService honorificService,
    PenumbraService penumbraService,
    StatusManager statusManager,
    HypnosisManager hypnosisManager,
    PossessionManager possessionManager)
{
    /// <summary>
    ///     Removes the status and clears any affected resources
    /// </summary>
    public async Task ClearCustomizePlus()
    {
        var result = await customizePlusService.DeleteTemporaryCustomizeAsync().ConfigureAwait(false);
        if (result)
            statusManager.ClearCustomizePlus();
    }
    
    /// <summary>
    ///     Removes the status and clears any affected resources
    /// </summary>
    public async Task ClearGlamourerPenumbra()
    {
        var glamourer = await glamourerService.RevertToAutomation(0).ConfigureAwait(false);
        var penumbra = await penumbraService.CallRemoveTemporaryMod().ConfigureAwait(false);
        if (glamourer && penumbra)
            statusManager.ClearGlamourerPenumbra();
    }
    
    /// <summary>
    ///     Removes the status and clears any affected resources
    /// </summary>
    public void ClearHonorific()
    {
        var result = honorificService.ClearCharacterTitle();
        if (result)
            statusManager.ClearHonorific();
    }
    
    /// <summary>
    ///     Removes the status and clears any affected resources
    /// </summary>
    public void ClearHypnosis()
    {
        var result = hypnosisManager.Wake();
        if (result)
            statusManager.ClearHypnosis();
    }
    
    /// <summary>
    ///     Removes the status and clears any affected resources
    /// </summary>
    public async Task ClearPossession()
    {
        var result = await possessionManager.EndAllParanormalActivity(true).ConfigureAwait(false);
        if (result)
            statusManager.ClearPossession();
    }
}