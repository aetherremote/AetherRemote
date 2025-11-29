using System.Numerics;
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
            ImGui.InputTextWithHint("##ProfileSearchBar", "Search", ref controller.SearchTerm, 32);

            ImGui.SameLine();

            if (SharedUserInterfaces.IconButton(FontAwesomeIcon.Sync, null, "Refresh Profiles"))
                controller.RefreshCustomizeProfiles();
        });

        var headerHeight = ImGui.GetCursorPosY() - begin;
        var moodlesContextBoxSize = new Vector2(0, ImGui.GetWindowHeight() - headerHeight - padding.X * 3 - SendProfileButtonHeight);
        if (ImGui.BeginChild("##MoodlesContextBoxDisplay", moodlesContextBoxSize, true, ImGuiWindowFlags.NoScrollbar))
        {
            ImGui.EndChild();
        }
        
        ImGui.Spacing();
        
        SharedUserInterfaces.ContentBox("MoodlesSend", AetherRemoteStyle.PanelBackground, false, () =>
        {
            if (selectionManager.Selected.Count is 0)
            {
                ImGui.BeginDisabled();
                ImGui.Button("You must select at least one friend", new Vector2(ImGui.GetWindowWidth() - padding.X * 2, SendProfileButtonHeight));
                ImGui.EndDisabled();
            }
            else if (controller.GetFriendsLackingPermissions().Count is not 0)
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
                        controller.SendCustomizeProfile();
                }
            }
        });

        ImGui.EndChild();
        ImGui.SameLine();
        friendsList.Draw();
    }
}