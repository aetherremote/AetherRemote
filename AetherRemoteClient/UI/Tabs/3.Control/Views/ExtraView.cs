using AetherRemoteClient.Accessors.Glamourer;
using AetherRemoteClient.Domain;
using AetherRemoteClient.Domain.Log;
using AetherRemoteClient.Domain.UI;
using AetherRemoteClient.Providers;
using AetherRemoteClient.UI.Tabs.Managers;
using AetherRemoteClient.UI.Tabs.Modules;
using AetherRemoteCommon.Domain;
using AetherRemoteCommon.Domain.CommonGlamourerApplyType;
using Dalamud.Interface;
using Dalamud.Interface.Colors;
using ImGuiNET;
using System;
using System.Collections.Generic;

namespace AetherRemoteClient.UI.Tabs.Views;

public class ExtraView : IControlTableModule
{
    // Injected
    private readonly ClientDataManager clientDataManager;
    private readonly CommandLockoutManager commandLockoutManager;

    // Instantiated
    private readonly ExtraManager extraManager;

    private bool includeSelfInBodySwap = false;
    private bool includeModSwapWithBodySwap = false;
    private bool includeModSwapWithTwinning = false;

    public ExtraView(
        ClientDataManager clientDataManager,
        CommandLockoutManager commandLockoutManager,
        GlamourerAccessor glamourerAccessor,
        HistoryLogManager historyLogManager,
        ModSwapManager modSwapManager,
        NetworkProvider networkProvider)
    {
        this.clientDataManager = clientDataManager;
        this.commandLockoutManager = commandLockoutManager;
        extraManager = new ExtraManager(clientDataManager, commandLockoutManager, glamourerAccessor, historyLogManager, modSwapManager, networkProvider);
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
                _ = extraManager.BodySwap(includeSelfInBodySwap, includeModSwapWithBodySwap);
        });

        SharedUserInterfaces.CommandDescription(
            description: "Attempts to swap bodies of selected targets randomly, optionally including yourself.",
            requiredPlugins: ["Glamourer", "Mare Synchronos"],
            requiredPermissions: ["Customization", "Equipment"]);

        var missingBodySwapPermissions = new List<string>();
        foreach (var target in clientDataManager.TargetManager.Targets)
        {
            if (PermissionChecker.HasValidTransformPermissions(GlamourerApplyFlag.All, target.Value.PermissionsGrantedByFriend) == false)
                missingBodySwapPermissions.Add(target.Key);
        }

        if (missingBodySwapPermissions.Count > 0)
        {
            ImGui.SameLine();
            SharedUserInterfaces.PermissionsWarning(missingBodySwapPermissions);
        }

        ImGui.Checkbox("Include Self", ref includeSelfInBodySwap);
        SharedUserInterfaces.Tooltip("Should you be included in the bodies to shuffle?");

        ImGui.SameLine();
        ImGui.Checkbox("Swap Mods##BodySwapSwapMods", ref includeModSwapWithBodySwap);
        SharedUserInterfaces.Tooltip(["Should the mods of the people being targetted be swapped as well?", "WARNING - HIGHLY EXPERIMENTAL"]);

        SharedUserInterfaces.DisableIf(commandLockoutManager.IsLocked, () =>
        {
            if (ImGui.Button("Twinning"))
                _ = extraManager.Twinning(includeModSwapWithTwinning);
        });

        SharedUserInterfaces.CommandDescription(
            description: "Attempts to transform selected targets into you.",
            requiredPlugins: ["Glamourer", "Mare Synchronos"],
            requiredPermissions: ["Customization", "Equipment"]);

        var missingTwinningPermissions = new List<string>();
        foreach (var target in clientDataManager.TargetManager.Targets)
        {
            if (PermissionChecker.HasValidTransformPermissions(GlamourerApplyFlag.All, target.Value.PermissionsGrantedByFriend) == false)
                missingTwinningPermissions.Add(target.Key);
        }

        if (missingTwinningPermissions.Count > 0)
        {
            ImGui.SameLine();
            SharedUserInterfaces.PermissionsWarning(missingTwinningPermissions);
        }
        ImGui.BeginDisabled();
        ImGui.Checkbox("Swap Mods##TwinningSwapMods", ref includeModSwapWithTwinning);
        ImGui.EndDisabled();
        SharedUserInterfaces.Tooltip(["Should the mods of the people being targetted be swapped as well?", "WARNING - HIGHLY EXPERIMENTAL"]);
    }

    public void Dispose() => GC.SuppressFinalize(this);
}
