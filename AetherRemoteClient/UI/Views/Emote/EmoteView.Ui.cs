using System.Numerics;
using AetherRemoteClient.UI.Style;
using AetherRemoteClient.Utils;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
using Dalamud.Interface.Colors;

namespace AetherRemoteClient.UI.Views.Emote;

public partial class EmoteView
{
    public void Draw()
    {
        ImGui.BeginChild("EmoteContent", AetherRemoteDimensions.ContentSize, false, AetherRemoteImGui.ContentFlags);

        switch (_selectionManager.Selected.Count)
        {
            case 0:
                SharedUserInterfaces.ContentBox("EmoteSelectMoreFriends", AetherRemoteColors.PanelColor, true,
                    () =>
                    {
                        SharedUserInterfaces.TextCentered("You must select at least one friend");
                    });

                ImGui.EndChild();
                ImGui.SameLine();
                _friendsList.Draw();
                return;

            case > 3:
                SharedUserInterfaces.ContentBox("EmoteLimitedSelection", AetherRemoteColors.PanelColor, true,
                    () =>
                    {
                        SharedUserInterfaces.TextCentered("You may only select 3 friends for in game functions");
                    });

                ImGui.EndChild();
                ImGui.SameLine();
                _friendsList.Draw();
                return;
        }

        SharedUserInterfaces.ContentBox("EmoteOptions", AetherRemoteColors.PanelColor, true, () =>
        {
            SharedUserInterfaces.MediumText("Options");
            ImGui.Checkbox("Display log message?", ref _displayLogMessage);
        });

        var friendsLackingPermissions = GetFriendsLackingPermissions();
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
            SharedUserInterfaces.ComboWithFilter("##EmoteSelector", "Search emotes", ref _emoteSelection, width, _emotesListFilter);

            ImGui.Spacing();

            if (_commandLockoutService.IsLocked)
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
                
                _ = Send().ConfigureAwait(false);
            }
        });

        ImGui.EndChild();
        ImGui.SameLine();
        _friendsList.Draw();
    }
}