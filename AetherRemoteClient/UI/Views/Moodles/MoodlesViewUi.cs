using System.Numerics;
using AetherRemoteClient.Domain.Interfaces;
using AetherRemoteClient.Services;
using AetherRemoteClient.UI.Components.Friends;
using AetherRemoteClient.Utils;
using Dalamud.Interface;
using Dalamud.Interface.Colors;
using ImGuiNET;

namespace AetherRemoteClient.UI.Views.Moodles;

public class MoodlesViewUi(
    FriendsListComponentUi friendsList,
    MoodlesViewUiController controller,
    CommandLockoutService commandLockoutService,
    FriendsListService friendsListService) : IDrawable
{
    // Const
    private static readonly Vector2 IconSize = new(32);

    public void Draw()
    {
        ImGui.BeginChild("MoodlesContent", AetherRemoteStyle.ContentSize, false, AetherRemoteStyle.ContentFlags);
        
        if (friendsListService.Selected.Count is 0)
        {
            SharedUserInterfaces.ContentBox("MoodlesSelectMoreFriends", AetherRemoteStyle.PanelBackground, true, () =>
            {
                SharedUserInterfaces.TextCentered("You must select at least one friend");
            });

            ImGui.EndChild();
            ImGui.SameLine();
            friendsList.Draw();
            return;
        }
        
        var windowWidthHalf = ImGui.GetWindowWidth() * 0.5f;
        SharedUserInterfaces.ContentBox("MoodlesSavedTemplates", AetherRemoteStyle.PanelBackground, true, () =>
        {
            SharedUserInterfaces.MediumText("Saved Moodles");
            ImGui.TextColored(ImGuiColors.DalamudGrey, "Coming in a future update");
        });

        SharedUserInterfaces.ContentBox("MoodlesQuickActions", AetherRemoteStyle.PanelBackground, true, () =>
        {
            SharedUserInterfaces.MediumText("Quick Actions");

            if (SharedUserInterfaces.IconButton(FontAwesomeIcon.Paste, IconSize))
                controller.Moodle = ImGui.GetClipboardText();
            SharedUserInterfaces.Tooltip("Paste moodle data from your clipboard");
        });
        
        var friendsLackingPermissions = controller.GetFriendsLackingPermissions();
        if (friendsLackingPermissions.Count is not 0)
        {
            SharedUserInterfaces.ContentBox("MoodlesLackingPermissions", AetherRemoteStyle.PanelBackground, true, () =>
            {
                SharedUserInterfaces.MediumText("Lacking Permissions", ImGuiColors.DalamudYellow);
                ImGui.SameLine();
                ImGui.AlignTextToFramePadding();
                SharedUserInterfaces.Icon(FontAwesomeIcon.ExclamationTriangle, ImGuiColors.DalamudYellow);
                SharedUserInterfaces.Tooltip("Commands send to these people will not be processed");
                ImGui.TextWrapped(string.Join(", ", friendsLackingPermissions));
            });
        }

        SharedUserInterfaces.ContentBox("MoodlesSend", AetherRemoteStyle.PanelBackground, false, () =>
        {
            SharedUserInterfaces.MediumText("Moodle Data");

            var width = (windowWidthHalf - ImGui.GetStyle().WindowPadding.X) * 2;
            ImGui.SetNextItemWidth(width);
            var shouldSendMoodle = ImGui.InputTextWithHint("##MoodleData", "Moodle data", ref controller.Moodle, 5000,
                ImGuiInputTextFlags.EnterReturnsTrue);

            ImGui.Spacing();

            if (commandLockoutService.IsLocked)
            {
                ImGui.BeginDisabled();
                ImGui.Button("Apply Moodle", new Vector2(width, 0));
                ImGui.EndDisabled();
            }
            else
            {
                if (ImGui.Button("Apply Moodle", new Vector2(width, 0)))
                    shouldSendMoodle = true;

                if (shouldSendMoodle is false)
                    return;

                commandLockoutService.Lock();
                controller.SendMoodle();
            }
        });

        ImGui.EndChild();
        ImGui.SameLine();
        friendsList.Draw();
    }
}