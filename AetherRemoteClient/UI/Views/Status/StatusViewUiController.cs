using System.Threading.Tasks;
using AetherRemoteClient.Dependencies.CustomizePlus.Services;
using AetherRemoteClient.Dependencies.Glamourer.Services;
using AetherRemoteClient.Dependencies.Honorific.Services;
using AetherRemoteClient.Dependencies.Penumbra.Services;
using AetherRemoteClient.Managers;
using AetherRemoteClient.Managers.Possession;
using AetherRemoteClient.Services;

namespace AetherRemoteClient.UI.Views.Status;

public class StatusViewUiController(
    CustomizePlusService customizePlusService,
    GlamourerService glamourerService,
    HonorificService honorificService,
    PenumbraService penumbraService,
    StatusService statusService,
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
            statusService.CustomizePlus = null;
    }
    
    /// <summary>
    ///     Removes the status and clears any affected resources
    /// </summary>
    public async Task ClearGlamourerPenumbra()
    {
        var glamourer = await glamourerService.RevertToAutomation(0).ConfigureAwait(false);
        var penumbra = await penumbraService.CallRemoveTemporaryMod().ConfigureAwait(false);
        if (glamourer && penumbra)
            statusService.GlamourerPenumbra = null;
    }
    
    /// <summary>
    ///     Removes the status and clears any affected resources
    /// </summary>
    public void ClearHonorific()
    {
        var result = honorificService.ClearCharacterTitle();
        if (result)
            statusService.Honorific = null;
    }
    
    /// <summary>
    ///     Removes the status and clears any affected resources
    /// </summary>
    public void ClearHypnosis()
    {
        var result = hypnosisManager.Wake();
        if (result)
            statusService.Hypnosis = null;
    }
    
    /// <summary>
    ///     Removes the status and clears any affected resources
    /// </summary>
    public async Task ClearPossession()
    {
        var result = await possessionManager.EndAllParanormalActivity(true).ConfigureAwait(false);
        if (result)
            statusService.Possession = null;
    }
}