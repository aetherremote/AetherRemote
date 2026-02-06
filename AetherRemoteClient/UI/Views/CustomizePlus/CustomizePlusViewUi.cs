using System.Collections.Generic;
using System.Numerics;
using AetherRemoteClient.Dependencies.CustomizePlus.Domain;
using AetherRemoteClient.Domain;
using AetherRemoteClient.Domain.Interfaces;
using AetherRemoteClient.Managers;
using AetherRemoteClient.Services;
using AetherRemoteClient.UI.Components.Friends;
using AetherRemoteClient.Utils;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface;

namespace AetherRemoteClient.UI.Views.CustomizePlus;

public class CustomizePlusViewUi(
    FriendsListComponentUi friendsList,
    CustomizePlusViewUiController controller,
    CommandLockoutService commandLockoutService,
    SelectionManager selectionManager) : IDrawable
{
    // Const
    private const int SendProfileButtonHeight = 40;
    
    public void Draw()
    {
        ImGui.BeginChild("CustomizePlusContent", AetherRemoteStyle.ContentSize, false, AetherRemoteStyle.ContentFlags);
        
        var width = ImGui.GetWindowWidth();
        var padding = new Vector2(ImGui.GetStyle().WindowPadding.X, 0);

        var begin = ImGui.GetCursorPosY();
        SharedUserInterfaces.ContentBox("ProfileSearch", AetherRemoteStyle.PanelBackground, true, () =>
        {
            SharedUserInterfaces.MediumText("Select Profile");

            ImGui.SetNextItemWidth(width - padding.X * 4 - ImGui.GetFontSize());
            if (ImGui.InputTextWithHint("##ProfileSearchBar", "Search", ref controller.SearchTerm, 32))
                controller.FilterProfilesBySearchTerm();

            ImGui.SameLine();

            if (SharedUserInterfaces.IconButton(FontAwesomeIcon.Sync, null, "Refresh Profiles"))
                _ = controller.RefreshCustomizeProfiles();
        });

        var headerHeight = ImGui.GetCursorPosY() - begin;
        var profilesContextBoxSize = new Vector2(0, ImGui.GetWindowHeight() - headerHeight - padding.X * 3 - SendProfileButtonHeight);
        if (ImGui.BeginChild("##ProfilesContextBoxDisplay", profilesContextBoxSize, true, ImGuiWindowFlags.NoScrollbar))
        {
            if (controller.Profiles is { } profiles)
                DrawTree(profiles);
            
            ImGui.EndChild();
        }
        
        ImGui.Spacing();
        
        SharedUserInterfaces.ContentBox("CustomizePlusSend", AetherRemoteStyle.PanelBackground, false, () =>
        {
            if (selectionManager.Selected.Count is 0)
            {
                ImGui.BeginDisabled();
                ImGui.Button("You must select at least one friend", new Vector2(ImGui.GetWindowWidth() - padding.X * 2, SendProfileButtonHeight));
                ImGui.EndDisabled();
            }
            else if (controller.MissingPermissionsForATarget())
            {
                ImGui.BeginDisabled();
                ImGui.Button("You lack permissions for one or more of your targets", new Vector2(ImGui.GetWindowWidth() - padding.X * 2, SendProfileButtonHeight));
                ImGui.EndDisabled();
            }
            else
            {
                if (commandLockoutService.IsLocked)
                {
                    ImGui.BeginDisabled();
                    ImGui.Button("Send Customize Profile", new Vector2(ImGui.GetWindowWidth() - padding.X * 2, SendProfileButtonHeight));
                    ImGui.EndDisabled();
                }
                else
                {
                    if (ImGui.Button("Send Customize Profile", new Vector2(ImGui.GetWindowWidth() - padding.X * 2, SendProfileButtonHeight)))
                        _ = controller.SendCustomizeProfile().ConfigureAwait(false);
                }
            }
        });

        ImGui.EndChild();
        ImGui.SameLine();
        friendsList.Draw();
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
                if (controller.SelectedProfileId == node.Content?.Guid)
                {
                    ImGui.PushStyleColor(ImGuiCol.Header, AetherRemoteStyle.PrimaryColor);
                    ImGui.Selectable(node.Name, true);
                    ImGui.PopStyleColor();
                }
                else
                {
                    if (ImGui.Selectable(node.Name))
                        if (node.Content is { } profile)
                            controller.SelectedProfileId = profile.Guid;
                }
            }
        }
    }
}