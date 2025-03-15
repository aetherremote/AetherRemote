using System.Numerics;
using AetherRemoteClient.Domain.Interfaces;
using AetherRemoteClient.Services;
using AetherRemoteClient.Services.External;
using AetherRemoteClient.Utils;
using Dalamud.Interface;
using Dalamud.Interface.Colors;
using ImGuiNET;

namespace AetherRemoteClient.UI.Views.Moodles;

public class MoodlesViewUi(
    CommandLockoutService commandLockoutService,
    FriendsListService friendsListService,
    NetworkService networkService) : IDrawable
{
    // Const
    private static readonly Vector2 IconSize = new(32);

    // Instantiated
    private readonly MoodlesViewUiController _controller = new(friendsListService, networkService);

    public bool Draw()
    {
        ImGui.BeginChild("MoodlesContent", AetherRemoteStyle.ContentSize, false, AetherRemoteStyle.ContentFlags);

        var windowWidthHalf = ImGui.GetWindowWidth() * 0.5f;

        if (friendsListService.Selected.Count is 0)
        {
            SharedUserInterfaces.ContentBox(AetherRemoteStyle.PanelBackground,
                () => { SharedUserInterfaces.TextCentered("You must select at least one friend"); });

            ImGui.EndChild();
            return true;
        }

        SharedUserInterfaces.ContentBox(AetherRemoteStyle.PanelBackground, () =>
        {
            SharedUserInterfaces.MediumText("Saved Moodles");
            ImGui.TextColored(ImGuiColors.DalamudGrey, "Coming in a future update");
        });

        SharedUserInterfaces.ContentBox(AetherRemoteStyle.PanelBackground, () =>
        {
            SharedUserInterfaces.MediumText("Quick Actions");

            SharedUserInterfaces.IconButton(FontAwesomeIcon.Paste, IconSize);
            SharedUserInterfaces.Tooltip("Paste moodle data from your clipboard");
        });
        
        var friendsLackingPermissions = _controller.GetFriendsLackingPermissions();
        if (friendsLackingPermissions.Count is not 0)
        {
            SharedUserInterfaces.ContentBox(AetherRemoteStyle.PanelBackground, () =>
            {
                SharedUserInterfaces.MediumText("Lacking Permissions", ImGuiColors.DalamudYellow);
                ImGui.SameLine();
                ImGui.AlignTextToFramePadding();
                SharedUserInterfaces.Icon(FontAwesomeIcon.ExclamationTriangle, ImGuiColors.DalamudYellow);
                SharedUserInterfaces.Tooltip("Commands send to these people will not be processed");
                ImGui.TextWrapped(string.Join(", ", friendsLackingPermissions));
            });
        }

        SharedUserInterfaces.ContentBox(AetherRemoteStyle.PanelBackground, () =>
        {
            SharedUserInterfaces.MediumText("Moodle Data");

            var width = (windowWidthHalf - ImGui.GetStyle().WindowPadding.X) * 2;
            ImGui.SetNextItemWidth(width);
            var shouldSendMoodle = ImGui.InputTextWithHint("##MoodleData", "Moodle data", ref _controller.Moodle, 5000,
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
                _controller.SendMoodle();
            }
        });

        ImGui.EndChild();
        return true;
    }
}