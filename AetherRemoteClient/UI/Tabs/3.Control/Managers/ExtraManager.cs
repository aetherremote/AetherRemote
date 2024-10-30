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

public class ExtraManager
{
    // Injected
    private readonly ClientDataManager clientDataManager;
    private readonly CommandLockoutManager commandLockoutManager;
    private readonly GlamourerAccessor glamourerAccessor;
    private readonly HistoryLogManager historyLogManager;
    private readonly NetworkProvider networkProvider;

    public ExtraManager(
        ClientDataManager clientDataManager,
        CommandLockoutManager commandLockoutManager,
        GlamourerAccessor glamourerAccessor,
        HistoryLogManager historyLogManager,
        NetworkProvider networkProvider)
    {
        this.clientDataManager = clientDataManager;
        this.commandLockoutManager = commandLockoutManager;
        this.glamourerAccessor = glamourerAccessor;
        this.historyLogManager = historyLogManager;
        this.networkProvider = networkProvider;
    }

    public async Task Twinning()
    {
        if (Plugin.DeveloperMode)
            return;

        var characterName = Plugin.ClientState.LocalPlayer?.Name.ToString();
        if (characterName is null)
        {
            Plugin.Log.Warning($"[Twinning] Failure, no local body");
            return;
        }

        var characterData = await glamourerAccessor.GetDesignAsync(characterName).ConfigureAwait(false);
        if (characterData is null)
        {
            Plugin.Log.Warning($"[Twinning] Failure, unable to get glamourer data");
            return;
        }

        commandLockoutManager.Lock(Constraints.ExternalCommandCooldownInSeconds);

        var targets = clientDataManager.TargetManager.Targets.Keys.ToList();
        var request = new TransformRequest(targets, characterData, GlamourerApplyFlag.All);
        var result = await networkProvider.InvokeCommand<TransformRequest, TransformResponse>(Network.Commands.Transform, request).ConfigureAwait(false);
        if (result.Success == false)
            Plugin.Log.Warning($"[Twinning] Failure, {result.Message}");

        // TODO Logging
    }

    public async Task BodySwap(bool includeSelfInBodySwap)
    {
        if (Plugin.DeveloperMode)
            return;

        if (includeSelfInBodySwap)
            await BodySwapWithRequester().ConfigureAwait(false);
        else
            await BodySwapWithoutRequester().ConfigureAwait(false);
    }

    private async Task BodySwapWithoutRequester()
    {
        if (clientDataManager.TargetManager.Targets.Count < 2)
            return;

        commandLockoutManager.Lock(Constraints.ExternalCommandCooldownInSeconds);

        var targets = clientDataManager.TargetManager.Targets.Keys.ToList();
        var request = new BodySwapRequest(targets, null);
        var result = await networkProvider.InvokeCommand<BodySwapRequest, BodySwapResponse>(Network.Commands.BodySwap, request).ConfigureAwait(false);
        if (result.Success == false)
            Plugin.Log.Warning($"[Body Swap] Failure, {result.Message}");
    }

    private async Task BodySwapWithRequester()
    {
        var characterName = Plugin.ClientState.LocalPlayer?.Name.ToString();
        if (characterName is null)
        {
            Plugin.Log.Warning($"[Body Swap] Failure, no local body");
            return;
        }

        var characterData = await glamourerAccessor.GetDesignAsync(characterName).ConfigureAwait(false);
        if (characterData is null)
        {
            Plugin.Log.Warning($"[Body Swap] Failure, unable to get glamourer data");
            return;
        }

        commandLockoutManager.Lock(Constraints.ExternalCommandCooldownInSeconds);

        var targets = clientDataManager.TargetManager.Targets.Keys.ToList();
        var request = new BodySwapRequest(targets, characterData);
        var result = await networkProvider.InvokeCommand<BodySwapRequest, BodySwapResponse>(Network.Commands.BodySwap, request).ConfigureAwait(false);
        if (result.Success == false)
        {
            Plugin.Log.Warning($"[Body Swap] Failure, {result.Message}");
            return;
        }
        
        if (result.CharacterData is null)
        {
            Plugin.Log.Warning($"[Body Swap] Failure, body data invalid. Tell a developer!");
            return;
        }

        var glamourerResult = await glamourerAccessor.ApplyDesignAsync(characterName, result.CharacterData).ConfigureAwait(false);
        if (glamourerResult == false)
            Plugin.Log.Warning($"[Body Swap] Failure, failed to apply glamourer");

        // TODO: Logging
    }
}
