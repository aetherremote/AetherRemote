using System.Collections.Generic;
using System.Numerics;
using AetherRemoteClient.Domain.Interfaces;
using AetherRemoteClient.Managers;
using AetherRemoteClient.Style;
using AetherRemoteClient.UI.Components.Friends;
using AetherRemoteClient.Utils;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
using Dalamud.Interface.Colors;
using FFXIVClientStructs.FFXIV.Client.UI.Info;

namespace AetherRemoteClient.UI.Views.Friends.Ui;

public partial class FriendsViewUi(FriendsListComponentUi friendsList, FriendsViewUiController controller, SelectionManager selection) : IDrawable
{
    private bool _drawIndividuals = true;
    
    public void Draw()
    {
        ImGui.BeginChild("PermissionContent", AetherRemoteDimensions.ContentSize, false, AetherRemoteImGui.ContentFlags);

        var width = ImGui.GetWindowWidth();
        
        SharedUserInterfaces.ContentBox("PermissionsSelectMode", AetherRemoteColors.PanelColor, true, () =>
        {
            var buttonWidth = (width - 3 * AetherRemoteImGui.WindowPadding.X) * 0.5f;
            var buttonDimensions = new Vector2(buttonWidth, AetherRemoteDimensions.SendCommandButtonHeight);
            
            SharedUserInterfaces.PushMediumFont();
            SharedUserInterfaces.TextCentered("Permissions");
            SharedUserInterfaces.PopMediumFont();
            
            ImGui.SameLine(width - ImGui.GetFontSize() - AetherRemoteImGui.WindowPadding.X * 2);
            SharedUserInterfaces.Icon(FontAwesomeIcon.QuestionCircle);

            if (ImGui.IsItemHovered())
            {
                ImGui.SetNextWindowSize(AetherRemoteDimensions.Tooltip);
                ImGui.BeginTooltip();
                
                SharedUserInterfaces.MediumText("Tutorial");

                ImGui.Separator();
                ImGui.TextWrapped("When you set permissions for someone, either individual or global, YOU are setting the functionality that THEY can do to YOU.");
                
                ImGui.Separator();
                ImGui.TextWrapped("Global Permissions, or Default Permissions, are a set of permissions granted to everyone on your friends list. Enabling a permission via the checkmark allows everyone to do that feature to you.");
                
                ImGui.Separator();
                ImGui.TextWrapped("Individual Permissions are a set of permissions granted to one single person. Individual permissions can be:");
                ImGui.BulletText("Deny, overwriting a global permission");
                ImGui.BulletText("Default, use whatever the global permission is set to");
                ImGui.BulletText("Allow, grant this permission even if not selected in global permissions");
                
                ImGui.Separator();
                ImGui.TextWrapped("Make sure you save your permissions at the bottom of individual or global permission menus. Saving only saves the current permissions you are editing, or make sure if you intend to change both at once, you save on both menus.");
                
                ImGui.EndTooltip();
            }

            // Snapshot the value just in case
            var shouldDrawIndividual = _drawIndividuals;

            // Individual Button
            if (shouldDrawIndividual)
                ImGui.PushStyleColor(ImGuiCol.Button, AetherRemoteColors.PrimaryColor);
            
            if (ImGui.Button("Individual", buttonDimensions))
                _drawIndividuals = true;
            
            if (shouldDrawIndividual)
                ImGui.PopStyleColor();
            
            ImGui.SameLine();
            
            // Global Button
            if (shouldDrawIndividual is false)
                ImGui.PushStyleColor(ImGuiCol.Button, AetherRemoteColors.PrimaryColor);
            
            if (ImGui.Button("Global", buttonDimensions))
                _drawIndividuals = false;
            
            if (shouldDrawIndividual is false)
                ImGui.PopStyleColor();
        });

        bool pendingChanges;
        if (_drawIndividuals)
        {
            DrawIndividualPermissions(width);
            pendingChanges = controller.PendingChangesIndividual();
        }
        else
        {
            DrawGlobalPermissions(width);
            pendingChanges = controller.PendingChangesGlobal();
        }

        if (pendingChanges)
        {
            var drawList = ImGui.GetWindowDrawList();
            drawList.ChannelsSplit(2);

            drawList.ChannelsSetCurrent(1);
            var textSize = ImGui.CalcTextSize("Unsaved Changes");
            var pos = ImGui.GetWindowPos();
            var size = ImGui.GetWindowSize();
            var final = new Vector2(pos.X + (size.X - textSize.X) * 0.5f, pos.Y + textSize.Y + AetherRemoteImGui.WindowPadding.Y);
            drawList.AddText(final, ImGui.ColorConvertFloat4ToU32(Vector4.One), "Unsaved Changes");

            drawList.ChannelsSetCurrent(0);
            var min = final - AetherRemoteImGui.WindowPadding;
            var max = final + textSize + AetherRemoteImGui.WindowPadding;
            drawList.AddRectFilled(min, max, ImGui.ColorConvertFloat4ToU32(new Vector4(0, 0, 0, 1)), AetherRemoteImGui.ChildRounding);
            drawList.AddRect(min, max, ImGui.ColorConvertFloat4ToU32(ImGuiColors.DalamudOrange), AetherRemoteImGui.ChildRounding);
            drawList.AddRect(pos, pos + size, ImGui.ColorConvertFloat4ToU32(ImGuiColors.DalamudOrange), AetherRemoteImGui.ChildRounding);

            drawList.ChannelsMerge();
        }
        
        ImGui.EndChild();
        ImGui.SameLine();
        friendsList.Draw(true, true);
    }

    private static readonly Dictionary<uint, string?> LinkshellCache = []; 
    private static unsafe string? GetLinkshellName(uint index)
    {
        if (LinkshellCache.TryGetValue(index, out var name))
            return name;
        
        var instance = InfoProxyLinkshell.Instance();
        var info = instance->GetLinkshellInfo(index);
        
        var linkshell = info == null ? null : string.Concat("[", index, "]: ", instance->GetLinkshellName(info->Id).ToString());
        LinkshellCache[index] = linkshell;
        return linkshell;
    }

    private static readonly Dictionary<uint, string> CrossWorldCache = []; 
    private static unsafe string GetCrossWorldLinkshellName(uint index)
    {
        if (CrossWorldCache.TryGetValue(index, out var name))
            return name;
        
        var instance = InfoProxyCrossWorldLinkshell.Instance();
        var info = instance->GetCrossworldLinkshellName(index);

        var crossWorld = string.Concat("[", index, "]: ", info->ToString());
        CrossWorldCache[index] = crossWorld;
        return crossWorld;
    }
}