using System;
using System.Collections.Generic;
using System.Numerics;
using AetherRemoteClient.Accessors.Glamourer;
using AetherRemoteClient.Domain;
using AetherRemoteClient.Domain.Log;
using AetherRemoteClient.Domain.UI;
using AetherRemoteClient.Providers;
using AetherRemoteClient.UI.Tabs.Managers;
using AetherRemoteClient.UI.Tabs.Modules;
using AetherRemoteCommon;
using AetherRemoteCommon.Domain;
using AetherRemoteCommon.Domain.CommonGlamourerApplyType;
using Dalamud.Interface;
using Dalamud.Interface.Colors;
using ImGuiNET;

namespace AetherRemoteClient.UI.Tabs.Views;

public class TransformationView(
    ClientDataManager clientDataManager,
    CommandLockoutManager commandLockoutManager,
    GlamourerAccessor glamourerAccessor,
    HistoryLogManager historyLogManager,
    NetworkProvider networkProvider) : IControlTabView
{
    // Const
    private static readonly Vector2 TransformButtonSize = new(80, 0);
    private const GlamourerApplyFlag CustomizationAndEquipmentFlags = GlamourerApplyFlag.Once | GlamourerApplyFlag.Customization | GlamourerApplyFlag.Equipment;
    
    // Instantiated
    private readonly TransformationManager _transformationManager = new(clientDataManager, commandLockoutManager, glamourerAccessor, historyLogManager, networkProvider);
    
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
            | (_transformationManager.ApplyCustomization ? GlamourerApplyFlag.Customization : 0)
            | (_transformationManager.ApplyEquipment ? GlamourerApplyFlag.Equipment : 0);

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
            if (ImGui.InputTextWithHint("###GlamourerDataInput", "Enter glamourer data", ref _transformationManager.GlamourerData, Constraints.GlamourerDataCharLimit, ImGuiInputTextFlags.EnterReturnsTrue))
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
                _ = _transformationManager.CopyMyGlamourerDataAsync();

            SharedUserInterfaces.Tooltip("Copy my glamourer data");
            ImGui.SameLine();

            if (SharedUserInterfaces.IconButton(FontAwesomeIcon.Crosshairs))
                _ = _transformationManager.CopyTargetGlamourerData();

            SharedUserInterfaces.Tooltip("Copy my target's glamourer data");
            ImGui.SameLine();

            if (SharedUserInterfaces.IconButton(FontAwesomeIcon.Broom))
                _transformationManager.GlamourerData = string.Empty;

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
            ImGui.SetCursorPosX(ImGui.GetWindowWidth() - appearanceTextWidth - equipmentTextWidth - ImGui.GetFontSize() * 2 - ImGui.GetStyle().WindowPadding.X * 4 - ImGui.GetStyle().FramePadding.X);

            if (ImGui.Checkbox("Appearance", ref _transformationManager.ApplyCustomization))
            {
                if (_transformationManager is { ApplyCustomization: false, ApplyEquipment: false })
                    _transformationManager.ApplyEquipment = true;
            }

            ImGui.SameLine();

            if (ImGui.Checkbox("Equipment", ref _transformationManager.ApplyEquipment))
            {
                if (_transformationManager is { ApplyCustomization: false, ApplyEquipment: false })
                    _transformationManager.ApplyCustomization = true;
            }

            if (shouldProcessRevert != RevertType.None)
                _ = _transformationManager.Revert(shouldProcessRevert);

            if (shouldProcessBecomeCommand && isCommandLockout == false)
                _ = _transformationManager.Become();
        });
    }
    
    public void Dispose() => GC.SuppressFinalize(this);
}