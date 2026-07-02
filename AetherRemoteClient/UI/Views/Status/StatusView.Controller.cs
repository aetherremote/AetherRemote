using System.Threading.Tasks;
using AetherRemoteClient.Utils;

namespace AetherRemoteClient.UI.Views.Status;

public partial class StatusView
{
    /// <summary>
    ///     Removes the status and clears any affected resources
    /// </summary>
    private async Task ClearCustomizePlus()
    {
        var result = await _customizePlusService.DeleteTemporaryCustomizeAsync().ConfigureAwait(false);
        if (result)
            _statusService.ClearCustomizePlus();
    }
    
    /// <summary>
    ///     Removes the status and clears any affected resources
    /// </summary>
    private async Task ClearGlamourerPenumbra()
    {
        if (await DalamudUtilities.TryGetLocalPlayer().ConfigureAwait(false) is not { } localPlayer)
            return;

        if (await _glamourerService.RevertToAutomation(localPlayer.ObjectIndex).ConfigureAwait(false) is false)
            return;

        // If collections are set, try to remove
        if (_characterTransformationManager.TryGetCollectionThatHasAetherRemoteMods() is { } collection)
            if (await _penumbraService.RemoveTemporaryMod(collection).ConfigureAwait(false) is false)
                return;
        
        // If the mod removal process succeeded or exited gracefully, we are now in the clear to reset the status
        _statusService.ClearGlamourerPenumbra();
    }
    
    /// <summary>
    ///     Removes the status and clears any affected resources
    /// </summary>
    private void ClearHonorific()
    {
        var result = _honorificService.ClearCharacterTitle();
        if (result)
            _statusService.ClearHonorific();
    }
    
    /// <summary>
    ///     Removes the status and clears any affected resources
    /// </summary>
    private void ClearHypnosis()
    {
        var result = _hypnosisManager.Wake();
        if (result)
            _statusService.ClearHypnosis();
    }
    
    /// <summary>
    ///     Removes the status and clears any affected resources
    /// </summary>
    private async Task ClearPossession()
    {
        var result = await _possessionManager.EndAllParanormalActivity(true).ConfigureAwait(false);
        if (result)
            _statusService.ClearPossession();
    }
}