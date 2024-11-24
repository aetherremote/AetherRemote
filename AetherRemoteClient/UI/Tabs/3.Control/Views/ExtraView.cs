using AetherRemoteClient.Accessors.Glamourer;
using AetherRemoteClient.Domain;
using AetherRemoteClient.Domain.Log;
using AetherRemoteClient.Domain.UI;
using AetherRemoteClient.Providers;
using AetherRemoteClient.UI.Tabs.Managers;
using AetherRemoteClient.UI.Tabs.Modules;
using Dalamud.Interface;
using Dalamud.Interface.Colors;
using ImGuiNET;
using System;
using System.Collections.Generic;
using AetherRemoteCommon.Domain;
using AetherRemoteCommon.Domain.Permissions;

namespace AetherRemoteClient.UI.Tabs.Views;

public class ExtraView(
    ClientDataManager clientDataManager,
    CommandLockoutManager commandLockoutManager,
    GlamourerAccessor glamourerAccessor,
    HistoryLogManager historyLogManager,
    ModManager modManager,
    NetworkProvider networkProvider) : IControlTabView
{
    // Const
    private const PrimaryPermissions BodySwapWithModsPermissions = PrimaryPermissions.BodySwap | PrimaryPermissions.Mods;
    private const PrimaryPermissions TwinningWithModsPermissions = PrimaryPermissions.Twinning | PrimaryPermissions.Mods;
    
    // Instantiated
    private readonly ExtraManager _extraManager = new(clientDataManager, commandLockoutManager, glamourerAccessor, historyLogManager, modManager, networkProvider);
    
    public void Draw()
    {
        SharedUserInterfaces.MediumText("Extra", ImGuiColors.ParsedOrange);
        ImGui.SameLine();
        ImGui.AlignTextToFramePadding();
        SharedUserInterfaces.Icon(FontAwesomeIcon.QuestionCircle);
        SharedUserInterfaces.Tooltip("A collection of extra features or shortcuts");

        SharedUserInterfaces.DisableIf(commandLockoutManager.IsLocked, () =>
        {
            if (ImGui.Button("Body swap"))
                _ = _extraManager.BodySwap();
        });

        SharedUserInterfaces.CommandDescription(
            description: "Attempts to swap bodies of selected targets randomly, optionally including yourself.",
            requiredPlugins: ["Glamourer", "Mare Synchronos"],
            requiredPermissions: ["Customization", "Equipment"]);
        
        var missingBodySwapPermissions = new List<string>();
        foreach (var target in clientDataManager.TargetManager.Targets)
        {
            if (_extraManager.IncludeModSwapWithBodySwap)
            {
                if ((target.Value.PermissionsGrantedByFriend.Primary & BodySwapWithModsPermissions) != BodySwapWithModsPermissions)
                    missingBodySwapPermissions.Add(target.Key);
            }
            else
            {
                if (target.Value.PermissionsGrantedByFriend.Primary.HasFlag(PrimaryPermissions.BodySwap) is false)
                    missingBodySwapPermissions.Add(target.Key);
            }
        }

        if (missingBodySwapPermissions.Count > 0)
        {
            ImGui.SameLine();
            SharedUserInterfaces.PermissionsWarning(missingBodySwapPermissions);
        }

        ImGui.Checkbox("Include Self", ref _extraManager.IncludeSelfInBodySwap);
        SharedUserInterfaces.Tooltip("Should you be included in the bodies to shuffle?");

        ImGui.SameLine();
        ImGui.Checkbox("Swap Mods##BodySwapSwapMods", ref _extraManager.IncludeModSwapWithBodySwap);
        SharedUserInterfaces.Tooltip(["Should the mods of the people being targeted be swapped as well?", "WARNING - HIGHLY EXPERIMENTAL"]);

        SharedUserInterfaces.DisableIf(commandLockoutManager.IsLocked, () =>
        {
            if (ImGui.Button("Twinning"))
                _ = _extraManager.Twinning();
        });

        SharedUserInterfaces.CommandDescription(
            description: "Attempts to transform selected targets into you.",
            requiredPlugins: ["Glamourer", "Mare Synchronos"],
            requiredPermissions: ["Customization", "Equipment"]);

        var missingTwinningPermissions = new List<string>();
        foreach (var target in clientDataManager.TargetManager.Targets)
        {
            if (_extraManager.IncludeModSwapWithTwinning)
            {
                if ((target.Value.PermissionsGrantedByFriend.Primary & TwinningWithModsPermissions) != TwinningWithModsPermissions)
                    missingTwinningPermissions.Add(target.Key);
            }
            else
            {
                if (target.Value.PermissionsGrantedByFriend.Primary.HasFlag(PrimaryPermissions.Twinning) is false)
                    missingTwinningPermissions.Add(target.Key);
            }
        }

        if (missingTwinningPermissions.Count > 0)
        {
            ImGui.SameLine();
            SharedUserInterfaces.PermissionsWarning(missingTwinningPermissions);
        }
        
        ImGui.Checkbox("Swap Mods##TwinningSwapMods", ref _extraManager.IncludeModSwapWithTwinning);
        SharedUserInterfaces.Tooltip(["Should the mods of the people being targeted be swapped as well?", "WARNING - HIGHLY EXPERIMENTAL"]);
    }

    public void Dispose() => GC.SuppressFinalize(this);
}
