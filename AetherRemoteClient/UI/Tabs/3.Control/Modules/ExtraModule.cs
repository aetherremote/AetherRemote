using AetherRemoteClient.Accessors.Glamourer;
using AetherRemoteClient.Domain;
using AetherRemoteClient.Domain.Log;
using AetherRemoteClient.Domain.UI;
using AetherRemoteClient.Providers;
using AetherRemoteCommon;
using AetherRemoteCommon.Domain;
using AetherRemoteCommon.Domain.CommonGlamourerApplyType;
using AetherRemoteCommon.Domain.Network.Commands;
using Dalamud.Interface;
using Dalamud.Interface.Colors;
using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AetherRemoteClient.UI.Tabs.Modules;

public class ExtraModule : IControlTableModule
{
    // Const
    private const GlamourerApplyFlag TwinningApplyFlags = GlamourerApplyFlag.Once | GlamourerApplyFlag.Customization | GlamourerApplyFlag.Equipment;

    // Injected
    private readonly ClientDataManager clientDataManager;
    private readonly CommandLockoutManager commandLockoutManager;
    private readonly GlamourerAccessor glamourerAccessor;
    private readonly HistoryLogManager historyLogManager;
    private readonly NetworkProvider networkProvider;

    private bool includeSelfInBodySwap = false;

    public ExtraModule(
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

    public void Draw()
    {
        SharedUserInterfaces.MediumText("Extra", ImGuiColors.ParsedOrange);
        ImGui.SameLine();
        ImGui.AlignTextToFramePadding();
        SharedUserInterfaces.Icon(FontAwesomeIcon.QuestionCircle);
        SharedUserInterfaces.Tooltip("A collection of extra features or shortcuts");

        SharedUserInterfaces.DisableIf(commandLockoutManager.IsLocked, () =>
        {
            if (ImGui.Button("Bodyswap"))
                _ = ProcessBodySwap();
        });

        SharedUserInterfaces.CommandDescription(
            description: "Attempts to swap bodies of selected targets randomly, optionally including yourself.",
            requiredPlugins: ["Glamourer", "Mare Synchronos"],
            requiredPermissions: ["Customization", "Equipment"]);

        var missingBodySwapPermissions = new List<string>();
        foreach (var target in clientDataManager.TargetManager.Targets)
        {
            if (PermissionChecker.HasValidTransformPermissions(TwinningApplyFlags, target.Value.PermissionsGrantedByFriend) == false)
                missingBodySwapPermissions.Add(target.Key);
        }

        if (missingBodySwapPermissions.Count > 0)
        {
            ImGui.SameLine();
            SharedUserInterfaces.PermissionsWarning(missingBodySwapPermissions);
        }

        ImGui.Checkbox("Include Self", ref includeSelfInBodySwap);
        SharedUserInterfaces.Tooltip("Should you be included in the bodies to shuffle?");

        SharedUserInterfaces.DisableIf(commandLockoutManager.IsLocked, () =>
        {
            if (ImGui.Button("Twinning"))
                _ = ProcessTwinning();
        });

        var missingTwinningPermissions = new List<string>();
        foreach (var target in clientDataManager.TargetManager.Targets)
        {
            if (PermissionChecker.HasValidTransformPermissions(TwinningApplyFlags, target.Value.PermissionsGrantedByFriend) == false)
                missingTwinningPermissions.Add(target.Key);
        }

        if (missingTwinningPermissions.Count > 0)
        {
            ImGui.SameLine();
            SharedUserInterfaces.PermissionsWarning(missingTwinningPermissions);
        }

        SharedUserInterfaces.CommandDescription(
            description: "Attempts to transform selected targets into you.",
            requiredPlugins: ["Glamourer", "Mare Synchronos"],
            requiredPermissions: ["Customization", "Equipment"]);
    }

    private async Task ProcessTwinning()
    {
        var characterName = Plugin.ClientState.LocalPlayer?.Name.ToString();
        if (characterName == null)
        {
            Plugin.Log.Warning($"Unable to process twinning command, no local body!");
            return;
        }

        var characterData = await glamourerAccessor.GetDesignAsync(characterName).ConfigureAwait(false);
        if (characterData == null)
        {
            Plugin.Log.Warning($"Unable to process twinning command, unable to get glamourer data!");
            return;
        }

        // Initiate UI Lockout
        commandLockoutManager.Lock(Constraints.ExternalCommandCooldownInSeconds);

        var targets = clientDataManager.TargetManager.Targets.Keys.ToList();
        var successful = await IssueTransformCommand(targets, characterData, TwinningApplyFlags).ConfigureAwait(false);
        if (successful == false)
        {
            // TODO: Make a toast with what went wrong
        }
    }

    private async Task ProcessBodySwap()
    {
        if (includeSelfInBodySwap)
            await BodySwapWithoutRequester().ConfigureAwait(false);
        else
            await BodySwapWithRequester().ConfigureAwait(false);
    }

    private async Task BodySwapWithoutRequester()
    {
        if (clientDataManager.TargetManager.Targets.Count < 2)
            return;

        commandLockoutManager.Lock(Constraints.ExternalCommandCooldownInSeconds);

        var targets = clientDataManager.TargetManager.Targets.Keys.ToList();
        var (successful, newBodyData) = await IssueBodySwapCommand(targets).ConfigureAwait(false);
        if (successful)
        {
            var message = $"You issued {string.Join(", ", targets)} to swap bodies at random";
            Plugin.Log.Information(message);
            historyLogManager.LogHistory(message);
        }
        else
        {
            // TODO: Make a toast with what went wrong
        }
    }

    private async Task BodySwapWithRequester()
    {
        var characterName = Plugin.ClientState.LocalPlayer?.Name.ToString();
        if (characterName == null)
        {
            Plugin.Log.Warning($"Unable to process body swap command, no local body!");
            return;
        }

        var characterData = await glamourerAccessor.GetDesignAsync(characterName).ConfigureAwait(false);
        if (characterData == null)
        {
            Plugin.Log.Warning($"Unable to process body swap command, unable to get glamourer data!");
            return;
        }

        // Initiate UI Lockout
        commandLockoutManager.Lock(Constraints.ExternalCommandCooldownInSeconds);

        var targets = clientDataManager.TargetManager.Targets.Keys.ToList();
        var (successful, newBodyData) = await IssueBodySwapCommand(targets, characterData).ConfigureAwait(false);
        if (successful)
        {
            if (newBodyData == null)
            {
                // TODO: Toast explaining what went wrong
                Plugin.Log.Warning("Your new body was lost in the aether during transfer! Tell a developer!");
                return;
            }

            var result = await glamourerAccessor.ApplyDesignAsync(characterName, newBodyData).ConfigureAwait(false);
            if (result)
            {
                var message = $"You issued yourself and {string.Join(", ", targets)} to swap bodies";
                Plugin.Log.Information(message);
                historyLogManager.LogHistoryGlamourer(message, newBodyData);
            }
        }
        else
        {
            // TODO: Make a toast with what went wrong
        }
    }

    public async Task<bool> IssueTransformCommand(List<string> targets, string glamourerData, GlamourerApplyFlag applyType)
    {
        #pragma warning disable CS0162
        if (Plugin.DeveloperMode)
            return true;
        #pragma warning restore CS0162

        var request = new TransformRequest(targets, glamourerData, applyType);
        var result = await networkProvider.InvokeCommand<TransformRequest, TransformResponse>(Network.Commands.Transform, request);
        if (result.Success == false)
            Plugin.Log.Warning($"Issuing transform command unsuccessful: {result.Message}");

        return result.Success;
    }

    public async Task<(bool, string?)> IssueBodySwapCommand(List<string> targets, string? characterData = null)
    {
        #pragma warning disable CS0162
        if (Plugin.DeveloperMode)
            return (true, null);
        #pragma warning restore CS0162

        var request = new BodySwapRequest(targets, characterData);
        var result = await networkProvider.InvokeCommand<BodySwapRequest, BodySwapResponse>(Network.Commands.BodySwap, request);
        if (result.Success == false)
            Plugin.Log.Warning($"Issuing body swap command unsuccessful: {result.Message}");

        return (result.Success, result.CharacterData);
    }

    public void Dispose() => GC.SuppressFinalize(this);
}
