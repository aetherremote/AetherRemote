using System.Threading.Tasks;
using AetherRemoteClient.Managers;
using AetherRemoteClient.Managers.Possession;
using AetherRemoteClient.Services;
using AetherRemoteClient.Utils;

namespace AetherRemoteClient.UI.Views.Status;

public class StatusViewUiController(
    CustomizePlusService customizePlusService,
    GlamourerService glamourerService,
    HonorificService honorificService,
    PenumbraService penumbraService,
    StatusManager statusManager,
    HypnosisManager hypnosisManager,
    CharacterTransformationManager characterTransformationManager,
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
        if (await DalamudUtilities.TryGetLocalPlayer().ConfigureAwait(false) is not { } localPlayer)
            return;

        if (await glamourerService.RevertToAutomation(localPlayer.ObjectIndex).ConfigureAwait(false) is false)
            return;

        // If collections are set, try to remove
        if (characterTransformationManager.TryGetCollectionThatHasAetherRemoteMods() is { } collection)
            if (await penumbraService.RemoveTemporaryMod(collection).ConfigureAwait(false) is false)
                return;
        
        // If the mod removal process succeeded or exited gracefully, we are now in the clear to reset the status
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