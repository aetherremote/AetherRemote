using System.Numerics;
using AetherRemoteClient.Domain.Interfaces;
using AetherRemoteClient.Managers;
using AetherRemoteClient.Services;
using AetherRemoteClient.Style;
using AetherRemoteClient.UI.Components.Friends;
using AetherRemoteClient.Utils;
using AetherRemoteCommon;
using AetherRemoteCommon.Domain.Enums;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
using Dalamud.Interface.Colors;

namespace AetherRemoteClient.UI.Views.Speak;

public class SpeakViewUi(
    FriendsListComponentUi friendsList,
    SpeakViewUiController controller,
    CommandLockoutService commandLockoutService,
    SelectionManager selectionManager) : IDrawable
{
    private static readonly Vector2 IconSize = new(24);

    public void Draw()
    {
        ImGui.BeginChild("SpeakContent", AetherRemoteDimensions.ContentSize, false, AetherRemoteImGui.ContentFlags);

        switch (selectionManager.Selected.Count)
        {
            case 0:
                SharedUserInterfaces.ContentBox("SpeakSelectMoreFriends", AetherRemoteColors.PanelColor, true, () =>
                {
                    SharedUserInterfaces.TextCentered("You must select at least one friend");
                });

                ImGui.EndChild();
                ImGui.SameLine();
                friendsList.Draw();
                return;

            case > 3:
                SharedUserInterfaces.ContentBox("SpeakLimitedSelection", AetherRemoteColors.PanelColor, true, () =>
                {
                    SharedUserInterfaces.TextCentered("You may only select 3 friends for in game functions");
                });

                ImGui.EndChild();
                ImGui.SameLine();
                friendsList.Draw();
                return;
        }

        var windowPadding = ImGui.GetStyle().WindowPadding;
        var windowWidth = ImGui.GetWindowWidth();

        SharedUserInterfaces.ContentBox("SpeakChannel", AetherRemoteColors.PanelColor, true, () =>
        {
            SharedUserInterfaces.MediumText("Channel");

            ImGui.SetNextItemWidth(140);

            if (ImGui.Combo("##ChannelSelector", ref controller.ChannelSelectionIndex, controller.ChatModeOptions,
                    controller.ChatModeOptions.Length))
                controller.ChannelSelect = (ChatChannel)controller.ChannelSelectionIndex;
        });

        switch (controller.ChannelSelectionIndex)
        {
            case (int)ChatChannel.Linkshell or (int)ChatChannel.CrossWorldLinkshell:
                SharedUserInterfaces.ContentBox("SpeakLinkshell", AetherRemoteColors.PanelColor, true, () =>
                {
                    SharedUserInterfaces.MediumText("Linkshell Number");
                    ImGui.SetNextItemWidth(60);
                    ImGui.Combo("##LinkshellNumberSelector", ref controller.LinkshellSelection,
                        controller.LinkshellNumbers, 8);
                });
                break;

            case (int)ChatChannel.Tell:
                SharedUserInterfaces.ContentBox("SpeakTell", AetherRemoteColors.PanelColor, true, () =>
                {
                    SharedUserInterfaces.MediumText("Tell Target");
                    ImGui.SetNextItemWidth(180);
                    ImGui.InputTextWithHint("##Wa", "Character Name", ref controller.CharacterName, 200);
                    
                    ImGui.SameLine();
                    SharedUserInterfaces.Icon(FontAwesomeIcon.At);

                    ImGui.SameLine();
                    SharedUserInterfaces.ComboWithFilter("##TellTargetSelector", "World",
                        ref controller.WorldName,
                        windowWidth - windowPadding.X - ImGui.GetCursorPosX(), controller.WorldsListFilter);

                    ImGui.Spacing();
                    if (SharedUserInterfaces.IconButton(FontAwesomeIcon.User, IconSize))
                        controller.FillWithPlayerData();
                    SharedUserInterfaces.Tooltip("Fill the World and Character Name with your data");
                    ImGui.SameLine();
                    if (SharedUserInterfaces.IconButton(FontAwesomeIcon.Crosshairs, IconSize))
                        controller.FillWithTargetData();
                    SharedUserInterfaces.Tooltip("Fill the World and Character Name with your target's data");
                });
                break;
        }
        
        var friendsLackingPermissions = controller.GetFriendsLackingPermissions();
        if (friendsLackingPermissions.Count is not 0)
        {
            SharedUserInterfaces.ContentBox("SpeakLackingPermissions", AetherRemoteColors.PanelColor, true, () =>
            {
                SharedUserInterfaces.MediumText("Lacking Permissions", ImGuiColors.DalamudYellow);
                ImGui.SameLine();
                ImGui.AlignTextToFramePadding();
                SharedUserInterfaces.Icon(FontAwesomeIcon.ExclamationTriangle, ImGuiColors.DalamudYellow);
                SharedUserInterfaces.Tooltip("Commands send to these people will not be processed");
                ImGui.TextWrapped(string.Join(", ", friendsLackingPermissions));
            });
        }

        SharedUserInterfaces.ContentBox("SpeakSend", AetherRemoteColors.PanelColor, false, () =>
        {
            var width = windowWidth - windowPadding.X * 2;
            SharedUserInterfaces.MediumText("Message");
            ImGui.SetNextItemWidth(width);

            var shouldSendMessage = ImGui.InputTextWithHint("##MessageContent", "Message to send",
                ref controller.Message, Constraints.Speak.MessageMax, ImGuiInputTextFlags.EnterReturnsTrue);

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
                controller.SendMessage();
            }
        });

        ImGui.EndChild();
        ImGui.SameLine();
        friendsList.Draw();
    }
}