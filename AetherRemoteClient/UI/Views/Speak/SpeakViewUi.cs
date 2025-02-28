using System.Numerics;
using AetherRemoteClient.Domain.Interfaces;
using AetherRemoteClient.Services;
using AetherRemoteClient.Utils;
using AetherRemoteCommon.Domain.Enums;
using Dalamud.Interface;
using Dalamud.Interface.Colors;
using ImGuiNET;

namespace AetherRemoteClient.UI.Views.Speak;

public class SpeakViewUi(
    CommandLockoutService commandLockoutService,
    FriendsListService friendsListService,
    NetworkService networkService,
    WorldService worldService) : IDrawable
{
    private readonly SpeakViewUiController _controller = new(friendsListService, networkService, worldService);

    private static readonly Vector2 IconSize = new(24);

    public bool Draw()
    {
        ImGui.BeginChild("SpeakContent", AetherRemoteStyle.ContentSize, false, AetherRemoteStyle.ContentFlags);

        switch (friendsListService.Selected.Count)
        {
            case 0:
                SharedUserInterfaces.ContentBox(AetherRemoteStyle.PanelBackground,
                    () => { SharedUserInterfaces.TextCentered("You must select at least one friend"); });

                ImGui.EndChild();
                return true;

            case > 3:
                SharedUserInterfaces.ContentBox(AetherRemoteStyle.PanelBackground,
                    () =>
                    {
                        SharedUserInterfaces.TextCentered("You may only select 3 friends for in game functions");
                    });

                ImGui.EndChild();
                return true;
        }

        var windowPadding = ImGui.GetStyle().WindowPadding;
        var windowWidth = ImGui.GetWindowWidth();

        SharedUserInterfaces.ContentBox(AetherRemoteStyle.PanelBackground, () =>
        {
            SharedUserInterfaces.MediumText("Channel");

            ImGui.SetNextItemWidth(140);

            if (ImGui.Combo("##ChannelSelector", ref _controller.ChannelSelectionIndex, _controller.ChatModeOptions,
                    _controller.ChatModeOptions.Length))
                _controller.ChannelSelect = (ChatChannel)_controller.ChannelSelectionIndex;
        });

        switch (_controller.ChannelSelectionIndex)
        {
            case (int)ChatChannel.Linkshell or (int)ChatChannel.CrossWorldLinkshell:
                SharedUserInterfaces.ContentBox(AetherRemoteStyle.PanelBackground, () =>
                {
                    SharedUserInterfaces.MediumText("Linkshell Number");
                    ImGui.SetNextItemWidth(60);
                    ImGui.Combo("##LinkshellNumberSelector", ref _controller.LinkshellSelection,
                        _controller.LinkshellNumbers, 8);
                });
                break;

            case (int)ChatChannel.Tell:
                SharedUserInterfaces.ContentBox(AetherRemoteStyle.PanelBackground, () =>
                {
                    SharedUserInterfaces.MediumText("Tell Target");
                    ImGui.SetNextItemWidth(180);
                    ImGui.InputTextWithHint("##Wa", "Character Name", ref _controller.CharacterName, 200);
                    
                    ImGui.SameLine();
                    SharedUserInterfaces.Icon(FontAwesomeIcon.At);

                    ImGui.SameLine();
                    SharedUserInterfaces.ComboWithFilter("##TellTargetSelector", "World",
                        ref _controller.WorldName,
                        windowWidth - windowPadding.X - ImGui.GetCursorPosX(), _controller.WorldsListFilter);

                    ImGui.Spacing();
                    if (SharedUserInterfaces.IconButton(FontAwesomeIcon.User, IconSize))
                        _controller.FillWithPlayerData();
                    SharedUserInterfaces.Tooltip("Fill the World and Character Name with your data");
                    ImGui.SameLine();
                    if (SharedUserInterfaces.IconButton(FontAwesomeIcon.Crosshairs, IconSize))
                        _controller.FillWithTargetData();
                    SharedUserInterfaces.Tooltip("Fill the World and Character Name with your target's data");
                });
                break;
        }
        
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
            var width = windowWidth - windowPadding.X * 2;
            SharedUserInterfaces.MediumText("Message");
            ImGui.SetNextItemWidth(width);

            var shouldSendMessage = ImGui.InputTextWithHint("##MessageContent", "Message to send",
                ref _controller.Message, 440, ImGuiInputTextFlags.EnterReturnsTrue);

            ImGui.Spacing();
            
            if (commandLockoutService.IsLocked)
            {
                ImGui.BeginDisabled();
                ImGui.Button("Send", new Vector2(width, 0));
                ImGui.EndDisabled();
            }
            else
            {
                if (ImGui.Button("Send", new Vector2(width, 0)))
                    shouldSendMessage = true;

                if (shouldSendMessage is false)
                    return;
                
                commandLockoutService.Lock();
                _controller.SendMessage();
            }
        });

        ImGui.EndChild();
        return true;
    }
}