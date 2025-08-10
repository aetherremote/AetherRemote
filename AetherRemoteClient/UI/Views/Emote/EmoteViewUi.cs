using System.Numerics;
using AetherRemoteClient.Domain.Interfaces;
using AetherRemoteClient.Services;
using AetherRemoteClient.UI.Components.Friends;
using AetherRemoteClient.Utils;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
using Dalamud.Interface.Colors;

namespace AetherRemoteClient.UI.Views.Emote;

public class EmoteViewUi(
    FriendsListComponentUi friendsList,
    CommandLockoutService commandLockoutService,
    EmoteService emoteService,
    FriendsListService friendsListService,
    NetworkService networkService) : IDrawable
{
    private readonly EmoteViewUiController _controller = new(emoteService, friendsListService, networkService);

    public void Draw()
    {
        ImGui.BeginChild("EmoteContent", AetherRemoteStyle.ContentSize, false, AetherRemoteStyle.ContentFlags);

        switch (friendsListService.Selected.Count)
        {
            case 0:
                SharedUserInterfaces.ContentBox("EmoteSelectMoreFriends", AetherRemoteStyle.PanelBackground, true,
                    () =>
                    {
                        SharedUserInterfaces.TextCentered("You must select at least one friend");
                    });

                ImGui.EndChild();
                ImGui.SameLine();
                friendsList.Draw();
                return;

            case > 3:
                SharedUserInterfaces.ContentBox("EmoteLimitedSelection", AetherRemoteStyle.PanelBackground, true,
                    () =>
                    {
                        SharedUserInterfaces.TextCentered("You may only select 3 friends for in game functions");
                    });

                ImGui.EndChild();
                ImGui.SameLine();
                friendsList.Draw();
                return;
        }

        SharedUserInterfaces.ContentBox("EmoteOptions", AetherRemoteStyle.PanelBackground, true, () =>
        {
            SharedUserInterfaces.MediumText("Options");
            ImGui.Checkbox("Display log message?", ref _controller.DisplayLogMessage);
        });

        var friendsLackingPermissions = _controller.GetFriendsLackingPermissions();
        if (friendsLackingPermissions.Count is not 0)
        {
            SharedUserInterfaces.ContentBox("EmoteLackingPermissions", AetherRemoteStyle.PanelBackground, true, () =>
            {
                SharedUserInterfaces.MediumText("Lacking Permissions", ImGuiColors.DalamudYellow);
                ImGui.SameLine();
                ImGui.AlignTextToFramePadding();
                SharedUserInterfaces.Icon(FontAwesomeIcon.ExclamationTriangle, ImGuiColors.DalamudYellow);
                SharedUserInterfaces.Tooltip("Commands send to these people will not be processed");
                ImGui.TextWrapped(string.Join(", ", friendsLackingPermissions));
            });
        }

        SharedUserInterfaces.ContentBox("EmoteSend", AetherRemoteStyle.PanelBackground, true, () =>
        {
            SharedUserInterfaces.MediumText("Emote");

            var width = ImGui.GetWindowWidth() - ImGui.GetStyle().WindowPadding.X * 2;
            SharedUserInterfaces.ComboWithFilter("##EmoteSelector", "Search emotes", ref _controller.EmoteSelection,
                width, _controller.EmotesListFilter);

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
                _controller.Send();
            }
        });

        ImGui.EndChild();
        ImGui.SameLine();
        friendsList.Draw();
    }
}