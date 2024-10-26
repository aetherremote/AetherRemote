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
using System.Numerics;
using System.Threading.Tasks;

namespace AetherRemoteClient.UI.Tabs.Modules;

public class TransformationModule : IControlTableModule
{
    // Const
    private static readonly Vector2 TransformButtonSize = new(80, 0);
    private const GlamourerApplyFlag CustomizationFlags = GlamourerApplyFlag.Once | GlamourerApplyFlag.Customization;
    private const GlamourerApplyFlag EquipmentFlags = GlamourerApplyFlag.Once | GlamourerApplyFlag.Equipment;
    private const GlamourerApplyFlag CustomizationAndEquipmentFlags = GlamourerApplyFlag.Once | GlamourerApplyFlag.Customization | GlamourerApplyFlag.Equipment;

    // Injected
    private readonly ClientDataManager clientDataManager;
    private readonly CommandLockoutManager commandLockoutManager;
    private readonly GlamourerAccessor glamourerAccessor;
    private readonly HistoryLogManager historyLogManager;
    private readonly NetworkProvider networkProvider;

    // Variables - Glamourer
    private string glamourerData = "";
    private bool applyCustomization = true;
    private bool applyEquipment = true;

    public TransformationModule(
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
        var shouldProcessBecomeCommand = false;
        var glamourerInstalled = glamourerAccessor.IsGlamourerUsable;

        SharedUserInterfaces.MediumText("Transformation", glamourerInstalled ? ImGuiColors.ParsedOrange : ImGuiColors.DalamudGrey);
        ImGui.SameLine();

        SharedUserInterfaces.CommandDescriptionWithQuestionMark(
            description: "Attempts to change selected friend(s)' appearance and or equipment.",
            requiredPlugins: ["Glamourer", "Mare Synchronos"],
            optionalPermissions: ["Customization", "Equipment"]);

        var friendsMissingPermissions = new List<string>();
        foreach (var target in clientDataManager.TargetManager.Targets)
        {
            var glamourerApplyFlags = GlamourerApplyFlag.Once
            | (applyCustomization ? GlamourerApplyFlag.Customization : 0)
            | (applyEquipment ? GlamourerApplyFlag.Equipment : 0);

            if (glamourerApplyFlags == GlamourerApplyFlag.Once)
                glamourerApplyFlags = CustomizationAndEquipmentFlags;

            if (PermissionChecker.HasValidTransformPermissions(glamourerApplyFlags, target.Value.PermissionsGrantedByFriend) == false)
                friendsMissingPermissions.Add(target.Key);
        }

        if (friendsMissingPermissions.Count > 0)
        {
            ImGui.SameLine(ImGui.GetWindowWidth() - ImGui.GetFontSize() - (ImGui.GetStyle().WindowPadding.X * 1.5f));
            SharedUserInterfaces.PermissionsWarning(friendsMissingPermissions);
        }

        SharedUserInterfaces.DisableIf(glamourerInstalled == false, () =>
        {
            ImGui.SetNextItemWidth(-1 * (TransformButtonSize.X + ImGui.GetStyle().WindowPadding.X));
            if (ImGui.InputTextWithHint("###GlamourerDataInput", "Enter glamourer data", ref glamourerData, Constraints.GlamourerDataCharLimit, ImGuiInputTextFlags.EnterReturnsTrue))
                shouldProcessBecomeCommand = true;

            ImGui.SameLine();

            var isCommandLockout = commandLockoutManager.IsLocked;
            SharedUserInterfaces.DisableIf(isCommandLockout, () =>
            {
                if (ImGui.Button("Transform", TransformButtonSize))
                    shouldProcessBecomeCommand = true;
            });

            ImGui.Spacing();

            if (SharedUserInterfaces.IconButton(FontAwesomeIcon.User))
                _ = CopyMyGlamourerDataAsync();

            SharedUserInterfaces.Tooltip("Copy my glamourer data");
            ImGui.SameLine();

            if (SharedUserInterfaces.IconButton(FontAwesomeIcon.Crosshairs))
                _ = CopyTargetGlamourerData();

            SharedUserInterfaces.Tooltip("Copy my target's glamourer data");
            ImGui.SameLine();

            if (SharedUserInterfaces.IconButton(FontAwesomeIcon.Broom))
                glamourerData = string.Empty;

            SharedUserInterfaces.Tooltip("Clear glamourer data input field");
            ImGui.SameLine();

            var shouldProcessRevert = RevertType.None;
            SharedUserInterfaces.DisableIf(isCommandLockout, () =>
            {
                if (SharedUserInterfaces.IconButton(FontAwesomeIcon.UserClock))
                    shouldProcessRevert = RevertType.Game;

                SharedUserInterfaces.Tooltip("Revert to Game");
                ImGui.SameLine();

                if (SharedUserInterfaces.IconButton(FontAwesomeIcon.ArrowsSpin))
                    shouldProcessRevert = RevertType.Automation;

                SharedUserInterfaces.Tooltip("Revert to Automation");
                ImGui.SameLine();
            });

            var appearanceTextWidth = ImGui.CalcTextSize("Appearance").X;
            var equipmentTextWidth = ImGui.CalcTextSize("Equipment").X;
            ImGui.SetCursorPosX(ImGui.GetWindowWidth() - appearanceTextWidth - equipmentTextWidth - (ImGui.GetFontSize() * 2) - (ImGui.GetStyle().WindowPadding.X * 4) - ImGui.GetStyle().FramePadding.X);

            if (ImGui.Checkbox("Appearance", ref applyCustomization))
            {
                if (applyCustomization == false && applyEquipment == false)
                    applyEquipment = true;
            }

            ImGui.SameLine();

            if (ImGui.Checkbox("Equipment", ref applyEquipment))
            {
                if (applyCustomization == false && applyEquipment == false)
                    applyCustomization = true;
            }

            if (shouldProcessRevert != RevertType.None)
                _ = ProcessRevertCommand(shouldProcessRevert);

            if (shouldProcessBecomeCommand && isCommandLockout == false)
                _ = ProcessBecomeCommand();
        });
    }

    private async Task ProcessRevertCommand(RevertType revertType)
    {
        // Initiate UI Lockout
        commandLockoutManager.Lock(Constraints.ExternalCommandCooldownInSeconds);

        var targets = clientDataManager.TargetManager.Targets.Keys.ToList();

        var request = new RevertRequest(targets, revertType);
        var result = await networkProvider.InvokeCommand<RevertRequest, RevertResponse>(Network.Commands.Revert, request);
        if (result.Success)
        {
            var targetNames = string.Join(", ", targets);
            var logMessage = revertType switch
            {
                RevertType.Automation => $"You issued {targetNames} to revert to their automations",
                RevertType.Game => $"You issued {targetNames} to revert to game",
                _ => $"You issued {targetNames} to revert"
            };

            Plugin.Log.Information(logMessage);
            historyLogManager.LogHistory(logMessage);
        }
        else
        {
            // TODO: Make a toast with what went wrong
            Plugin.Log.Warning($"Issuing revert command unsuccessful: {result.Message}");
        }
    }

    private async Task ProcessBecomeCommand()
    {
        if (glamourerData.Length == 0)
            return;

        var glamourerApplyFlags = GlamourerApplyFlag.Once
            | (applyCustomization ? GlamourerApplyFlag.Customization : 0)
            | (applyEquipment ? GlamourerApplyFlag.Equipment : 0);

        if (glamourerApplyFlags == GlamourerApplyFlag.Once)
            glamourerApplyFlags = CustomizationAndEquipmentFlags;

        // Initiate UI Lockout
        commandLockoutManager.Lock(Constraints.ExternalCommandCooldownInSeconds);

        var targets = clientDataManager.TargetManager.Targets.Keys.ToList();
        var result = await IssueTransformCommand(targets, glamourerData, glamourerApplyFlags).ConfigureAwait(false);
        if (result)
        {
            var targetNames = string.Join(", ", targets);
            var logMessage = glamourerApplyFlags switch
            {
                CustomizationFlags => $"You issued {targetNames} to change their appearance",
                EquipmentFlags => $"You issued {targetNames} to change their outfit",
                CustomizationAndEquipmentFlags => $"You issued {targetNames} to change their outfit, and appearance",
                _ => $"You issued {targetNames} to change"
            };

            Plugin.Log.Information(logMessage);
            historyLogManager.LogHistoryGlamourer(logMessage, glamourerData);

            // Reset glamourer data
            glamourerData = string.Empty;
        }
        else
        {
            // TODO: Make a toast with what went wrong
        }
    }

    private async Task CopyMyGlamourerDataAsync()
    {
        var playerName = Plugin.ClientState.LocalPlayer?.Name.ToString();
        if (playerName == null)
            return;

        glamourerData = await glamourerAccessor.GetDesignAsync(playerName).ConfigureAwait(false) ?? string.Empty;
    }

    private async Task CopyTargetGlamourerData()
    {
        var targetName = Plugin.TargetManager.Target?.Name.ToString();
        if (targetName == null)
            return;

        glamourerData = await glamourerAccessor.GetDesignAsync(targetName).ConfigureAwait(false) ?? string.Empty;
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

    public void Dispose() => GC.SuppressFinalize(this);
}
