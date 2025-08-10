using System.Numerics;
using AetherRemoteClient.Domain.Interfaces;
using AetherRemoteClient.Services;
using AetherRemoteClient.UI.Components.Friends;
using AetherRemoteClient.Utils;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
using Dalamud.Interface.Colors;

namespace AetherRemoteClient.UI.Views.CustomizePlus;

public class CustomizePlusViewUi(
    FriendsListComponentUi friendsList,
    CustomizePlusViewUiController controller,
    CommandLockoutService commandLockoutService,
    FriendsListService friendsListService) : IDrawable
{
    // Const
    private static readonly Vector2 IconSize = new(32);
    
    public void Draw()
    {
        ImGui.BeginChild("BodySwapContent", AetherRemoteStyle.ContentSize, false, AetherRemoteStyle.ContentFlags);
        
        if (friendsListService.Selected.Count is 0)
        {
            SharedUserInterfaces.ContentBox("CustomizeSelectMoreFriends", AetherRemoteStyle.PanelBackground, true, () =>
            {
                SharedUserInterfaces.TextCentered("You must select at least one friend");
            });

            ImGui.EndChild();
            ImGui.SameLine();
            friendsList.Draw();
            return;
        }
        
        var windowWidthHalf = ImGui.GetWindowWidth() * 0.5f;
        SharedUserInterfaces.ContentBox("CustomizeSavedTemplates", AetherRemoteStyle.PanelBackground, true, () =>
        {
            SharedUserInterfaces.MediumText("Saved Templates");
            ImGui.TextColored(ImGuiColors.DalamudGrey, "Coming in a future update");
        });

        SharedUserInterfaces.ContentBox("CustomizeQuickActions", AetherRemoteStyle.PanelBackground, true, () =>
        {
            SharedUserInterfaces.MediumText("Quick Actions");
            if (SharedUserInterfaces.IconButton(FontAwesomeIcon.Paste, IconSize))
                controller.CustomizeData = ImGui.GetClipboardText();
            SharedUserInterfaces.Tooltip("Paste Customize+ data from clipboard");
        });
        
        var friendsLackingPermissions = controller.GetFriendsLackingPermissions();
        if (friendsLackingPermissions.Count is not 0)
        {
            SharedUserInterfaces.ContentBox("CustomizeLackingPermissions", AetherRemoteStyle.PanelBackground, true, () =>
            {
                SharedUserInterfaces.MediumText("Lacking Permissions", ImGuiColors.DalamudYellow);
                ImGui.SameLine();
                ImGui.AlignTextToFramePadding();
                SharedUserInterfaces.Icon(FontAwesomeIcon.ExclamationTriangle, ImGuiColors.DalamudYellow);
                SharedUserInterfaces.Tooltip("Commands send to these people will not be processed");
                ImGui.TextWrapped(string.Join(", ", friendsLackingPermissions));
            });
        }
        
        SharedUserInterfaces.ContentBox("CustomizeSend", AetherRemoteStyle.PanelBackground, false, () =>
        {
            SharedUserInterfaces.MediumText("Customize Data");

            var width = (windowWidthHalf - ImGui.GetStyle().WindowPadding.X) * 2;
            ImGui.SetNextItemWidth(width);
            var shouldSendCustomize = ImGui.InputTextWithHint("##CustomizeData", "Customize data", ref controller.CustomizeData, 5000,
                ImGuiInputTextFlags.EnterReturnsTrue);

            ImGui.Spacing();

            if (commandLockoutService.IsLocked)
            {
                ImGui.BeginDisabled();
                ImGui.Button("Apply Customize", new Vector2(width, 0));
                ImGui.EndDisabled();
            }
            else
            {
                if (ImGui.Button("Apply Customize", new Vector2(width, 0)))
                    shouldSendCustomize = true;

                if (shouldSendCustomize is false)
                    return;

                commandLockoutService.Lock();
                controller.SendCustomize();
            }
        });
        
        ImGui.EndChild();
        ImGui.SameLine();
        friendsList.Draw();
    }
}