using System.Numerics;
using AetherRemoteClient.Domain.Interfaces;
using AetherRemoteClient.Managers;
using AetherRemoteClient.Services;
using AetherRemoteClient.Style;
using AetherRemoteClient.UI.Components.Friends;
using AetherRemoteClient.Utils;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
using Dalamud.Interface.Colors;

namespace AetherRemoteClient.UI.Views.Emote;

public class EmoteViewUi(
    FriendsListComponentUi friendsList,
    EmoteViewUiController controller,
    CommandLockoutService commandLockoutService,
    SelectionManager selectionManager) : IDrawable
{
    public void Draw()
    {
        ImGui.BeginChild("EmoteContent", AetherRemoteDimensions.ContentSize, false, AetherRemoteImGui.ContentFlags);

        switch (selectionManager.Selected.Count)
        {
            case 0:
                SharedUserInterfaces.ContentBox("EmoteSelectMoreFriends", AetherRemoteColors.PanelColor, true,
                    () =>
                    {
                        SharedUserInterfaces.TextCentered("You must select at least one friend");
                    });

                ImGui.EndChild();
                ImGui.SameLine();
                friendsList.Draw();
                return;

            case > 3:
                SharedUserInterfaces.ContentBox("EmoteLimitedSelection", AetherRemoteColors.PanelColor, true,
                    () =>
                    {
                        SharedUserInterfaces.TextCentered("You may only select 3 friends for in game functions");
                    });

                ImGui.EndChild();
                ImGui.SameLine();
                friendsList.Draw();
                return;
        }

        SharedUserInterfaces.ContentBox("EmoteOptions", AetherRemoteColors.PanelColor, true, () =>
        {
            SharedUserInterfaces.MediumText("Options");
            ImGui.Checkbox("Display log message?", ref controller.DisplayLogMessage);
        });

        var friendsLackingPermissions = controller.GetFriendsLackingPermissions();
        if (friendsLackingPermissions.Count is not 0)
        {
            SharedUserInterfaces.ContentBox("EmoteLackingPermissions", AetherRemoteColors.PanelColor, true, () =>
            {
                SharedUserInterfaces.MediumText("Lacking Permissions", ImGuiColors.DalamudYellow);
                ImGui.SameLine();
                ImGui.AlignTextToFramePadding();
                SharedUserInterfaces.Icon(FontAwesomeIcon.ExclamationTriangle, ImGuiColors.DalamudYellow);
                SharedUserInterfaces.Tooltip("Commands send to these people will not be processed");
                ImGui.TextWrapped(string.Join(", ", friendsLackingPermissions));
            });
        }

        SharedUserInterfaces.ContentBox("EmoteSend", AetherRemoteColors.PanelColor, true, () =>
        {
            SharedUserInterfaces.MediumText("Emote");

            var width = ImGui.GetWindowWidth() - ImGui.GetStyle().WindowPadding.X * 2;
            SharedUserInterfaces.ComboWithFilter("##EmoteSelector", "Search emotes", ref controller.EmoteSelection,
                width, controller.EmotesListFilter);

            ImGui.Spacing();

            if (commandLockoutService.IsLocked)
            {
                ImGui.BeginDisabled();
                ImGui.Button("Send", new Vector2(width, 0));
                ImGui.EndDisabled();
            }
            else
            {
                // If the button is not pressed, exit
                if (ImGui.Button("Send", new Vector2(width, 0)) is false)
                    return;

                commandLockoutService.Lock();
                controller.Send();
            }
        });

        ImGui.EndChild();
        ImGui.SameLine();
        friendsList.Draw();
    }
}