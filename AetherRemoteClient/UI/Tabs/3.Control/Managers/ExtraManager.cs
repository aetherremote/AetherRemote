using AetherRemoteClient.Accessors.Glamourer;
using AetherRemoteClient.Domain;
using AetherRemoteClient.Domain.Log;
using AetherRemoteClient.Providers;
using AetherRemoteCommon;
using AetherRemoteCommon.Domain.CommonGlamourerApplyType;
using AetherRemoteCommon.Domain.Network.Commands;
using System.Linq;
using System.Threading.Tasks;

namespace AetherRemoteClient.UI.Tabs.Managers;

public class ExtraManager(
    ClientDataManager clientDataManager,
    CommandLockoutManager commandLockoutManager,
    GlamourerAccessor glamourerAccessor,
    HistoryLogManager historyLogManager,
    ModSwapManager modSwapManager,
    NetworkProvider networkProvider)
{
    // TODO: Twinning mod swap
    public async Task Twinning(bool swapMods)
    {
        if (Plugin.DeveloperMode) return;
        if (Plugin.ClientState.LocalPlayer is null)
        {
            Plugin.Log.Warning("[Twinning] Failure, no local body");
            return;
        }

        var characterData = await glamourerAccessor.GetDesignAsync().ConfigureAwait(false);
        if (characterData is null)
        {
            Plugin.Log.Warning("[Twinning] Failure, unable to get glamourer data");
            return;
        }

        commandLockoutManager.Lock();

        var targets = clientDataManager.TargetManager.Targets.Keys.ToList();
        var request = new TransformRequest(targets, characterData, GlamourerApplyFlag.All);
        var result = await networkProvider.InvokeCommand<TransformRequest, TransformResponse>(Network.Commands.Transform, request).ConfigureAwait(false);
        if (result.Success == false)
            Plugin.Log.Warning($"[Twinning] Failure, {result.Message}");

        // TODO Logging
    }

    public async Task BodySwap(bool includeSelfInBodySwap, bool swapMods)
    {
        if (Plugin.DeveloperMode) return;
        
        if (includeSelfInBodySwap)
            await BodySwapWithRequester(swapMods).ConfigureAwait(false);
        else
            await BodySwapWithoutRequester(swapMods).ConfigureAwait(false);
    }

    private async Task BodySwapWithoutRequester(bool swapMods)
    {
        if (clientDataManager.TargetManager.Targets.Count < 2) return;

        commandLockoutManager.Lock();

        var targets = clientDataManager.TargetManager.Targets.Keys.ToList();
        var request = new BodySwapRequest(targets, swapMods, null, null);
        var result = await networkProvider.InvokeCommand<BodySwapRequest, BodySwapResponse>(Network.Commands.BodySwap, request).ConfigureAwait(false);
        if (result.Success == false)
            Plugin.Log.Warning($"[Body Swap] Failure, {result.Message}");
    }

    private async Task BodySwapWithRequester(bool swapMods)
    {
        string? characterName = null;
        if (swapMods)
        {
            characterName = Plugin.ClientState.LocalPlayer?.Name.ToString();
            if (characterName is null)
            {
                Plugin.Log.Warning("[Body Swap] Failure, no local body");
                return;
            }
        }

        var characterData = await glamourerAccessor.GetDesignAsync().ConfigureAwait(false);
        if (characterData is null)
        {
            Plugin.Log.Warning("[Body Swap] Failure, unable to get glamourer data");
            return;
        }

        commandLockoutManager.Lock();

        var targets = clientDataManager.TargetManager.Targets.Keys.ToList();
        var request = new BodySwapRequest(targets, swapMods, characterName, characterData);
        var result = await networkProvider.InvokeCommand<BodySwapRequest, BodySwapResponse>(Network.Commands.BodySwap, request).ConfigureAwait(false);
        if (result.Success == false)
        {
            Plugin.Log.Warning($"[Body Swap] Failure, {result.Message}");
            return;
        }
        
        if (result.CharacterData is null)
        {
            Plugin.Log.Warning("[Body Swap] Failure, body data invalid. Tell a developer!");
            return;
        }

        if (swapMods)
        {
            if (result.CharacterName is null)
            {
                Plugin.Log.Warning("[Body Swap] Failure, character name not included when it should have been. Tell a developer!");
                return;
            }

            await modSwapManager.SwapMods(result.CharacterName);
        }

        var glamourerResult = await glamourerAccessor.ApplyDesignAsync(result.CharacterData).ConfigureAwait(false);
        if (glamourerResult == false)
            Plugin.Log.Warning("[Body Swap] Failure, failed to apply glamourer");

        // TODO: Logging
    }
}
