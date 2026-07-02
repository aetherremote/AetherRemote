using System.Collections.Generic;
using System.Numerics;
using AetherRemoteClient.Domain;
using AetherRemoteClient.Domain.CustomizePlus;
using AetherRemoteClient.UI.Style;
using AetherRemoteClient.Utils;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
using Dalamud.Interface.Colors;

namespace AetherRemoteClient.UI.Views.CustomizePlus;

public partial class CustomizePlusView
{
    // Const
    private const int SendProfileButtonHeight = 40;
    
    public void Draw()
    {
        ImGui.BeginChild("CustomizePlusContent", AetherRemoteDimensions.ContentSize, false, AetherRemoteImGui.ContentFlags);
        
        var width = ImGui.GetWindowWidth();

        var begin = ImGui.GetCursorPosY();
        SharedUserInterfaces.ContentBox("ProfileSearch", AetherRemoteColors.PanelColor, true, () =>
        {
            SharedUserInterfaces.MediumText("Select Profile");

            ImGui.SetNextItemWidth(width - AetherRemoteImGui.WindowPadding.X * 4 - ImGui.GetFontSize());
            if (ImGui.InputTextWithHint("##ProfileSearchBar", "Search", ref _searchTerm, 32))
                FilterProfilesBySearchTerm();

            ImGui.SameLine();

            if (SharedUserInterfaces.IconButton(FontAwesomeIcon.Sync, null, "Refresh Profiles"))
                _ = RefreshCustomizeProfiles().ConfigureAwait(false);
        });
        
        var headerHeight = ImGui.GetCursorPosY() - begin;
        var applyModeHeight = ImGui.GetFontSize() * 2 + AetherRemoteImGui.ItemSpacing.Y * 2; 
        var padding = + AetherRemoteImGui.WindowPadding.Y * 7;
        var profilesContextBoxSize = new Vector2(0, ImGui.GetWindowHeight() - headerHeight - AetherRemoteDimensions.SendCommandButtonHeight - applyModeHeight - padding);
        if (ImGui.BeginChild("##ProfilesContextBoxDisplay", profilesContextBoxSize, true, ImGuiWindowFlags.NoScrollbar))
        {
            if (Profiles is { } profiles)
                DrawTree(profiles);
            
            ImGui.EndChild();
        }
        
        ImGui.Spacing();
        
        SharedUserInterfaces.ContentBox("CustomizePlusOptions", AetherRemoteColors.PanelColor, true, () =>
        {
            ImGui.AlignTextToFramePadding();
            ImGui.TextUnformatted("Apply Mode");
            ImGui.SameLine();
            SharedUserInterfaces.Icon(FontAwesomeIcon.QuestionCircle);
            if (ImGui.IsItemHovered())
            {
                ImGui.BeginTooltip();
                ImGui.TextUnformatted("Apply mode refers to how the customize profile should be applied");
                ImGui.Separator();
                ImGui.TextUnformatted("Default");
                ImGui.BulletText("Applies the profile as a new override of your current profiles");
                
                ImGui.TextUnformatted("Merge");
                ImGui.BulletText("Applies the profile by creating a new profile with your current profiles, and the applied profile");

                ImGui.TextColored(ImGuiColors.DalamudGrey, "Note: Profiles will not be edited or altered in any way");
                ImGui.EndTooltip();
            }
            
            ImGui.RadioButton("Default", ref _applyMode, CustomizePlusView.ApplyModeDefault);
            ImGui.SameLine(width * 0.5f);
            ImGui.RadioButton("Merge", ref _applyMode, CustomizePlusView.ApplyModeMerge);
        });
        
        SharedUserInterfaces.ContentBox("CustomizePlusSend", AetherRemoteColors.PanelColor, false, () =>
        {
            if (_selectionManager.Selected.Count is 0)
            {
                ImGui.BeginDisabled();
                ImGui.Button("You must select at least one friend", new Vector2(ImGui.GetWindowWidth() - AetherRemoteImGui.WindowPadding.X * 2, SendProfileButtonHeight));
                ImGui.EndDisabled();
            }
            else if (MissingPermissionsForATarget())
            {
                ImGui.BeginDisabled();
                ImGui.Button("You lack permissions for one or more of your targets", new Vector2(ImGui.GetWindowWidth() - AetherRemoteImGui.WindowPadding.X * 2, SendProfileButtonHeight));
                ImGui.EndDisabled();
            }
            else
            {
                if (_commandLockoutService.IsLocked)
                {
                    ImGui.BeginDisabled();
                    ImGui.Button("Send Customize Profile", new Vector2(ImGui.GetWindowWidth() - AetherRemoteImGui.WindowPadding.X * 2, SendProfileButtonHeight));
                    ImGui.EndDisabled();
                }
                else
                {
                    if (ImGui.Button("Send Customize Profile", new Vector2(ImGui.GetWindowWidth() - AetherRemoteImGui.WindowPadding.X * 2, SendProfileButtonHeight)))
                        _ = SendCustomizeProfile().ConfigureAwait(false);
                }
            }
        });

        ImGui.EndChild();
        ImGui.SameLine();
        _friendsListComponentUi.Draw();
    }

    /// <summary>
    ///     Renders a recursive tree view of the Customize+ profiles
    /// </summary>
    private void DrawTree(IEnumerable<FolderNode<Profile>> nodes)
    {
        foreach (var node in nodes)
        {
            // Folder node
            if (node.Content is null)
            {
                // Create the node
                // ReSharper disable once InvertIf
                if (ImGui.TreeNodeEx(node.Name, ImGuiTreeNodeFlags.SpanAvailWidth | ImGuiTreeNodeFlags.Framed))
                {
                    // Recursively draw the children inside the tree node
                    DrawTree(node.Children.Values);
                    
                    // Close the tree
                    ImGui.TreePop();
                }
            }
            // Leaf node, that contains the actual content
            else
            {
                if (_selectedProfileId == node.Content?.Guid)
                {
                    ImGui.PushStyleColor(ImGuiCol.Header, AetherRemoteColors.PrimaryColor);
                    ImGui.Selectable(node.Name, true);
                    ImGui.PopStyleColor();
                }
                else
                {
                    if (ImGui.Selectable(node.Name))
                        if (node.Content is { } profile)
                            _selectedProfileId = profile.Guid;
                }
            }
        }
    }
}