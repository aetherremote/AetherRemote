using System;
using System.Collections.Generic;
using System.Numerics;
using AetherRemoteClient.Domain;
using AetherRemoteClient.Domain.Log;
using AetherRemoteClient.Domain.UI;
using AetherRemoteClient.Providers;
using AetherRemoteClient.UI.Tabs.Managers;
using AetherRemoteClient.UI.Tabs.Modules;
using AetherRemoteClient.Uncategorized;
using AetherRemoteCommon;
using AetherRemoteCommon.Domain;
using AetherRemoteCommon.Domain.CommonChatMode;
using Dalamud.Interface;
using Dalamud.Interface.Colors;
using ImGuiNET;

namespace AetherRemoteClient.UI.Tabs.Views;

public class SpeakView(
    ClientDataManager clientDataManager,
    CommandLockoutManager commandLockoutManager,
    HistoryLogProvider historyLogProvider,
    NetworkProvider networkProvider,
    WorldProvider worldProvider) : IControlTabView
{
    // Const
    private static readonly Vector2 SendButtonSize = new(40, 0);
    
    // Instantiated
    private readonly SpeakManager _speakManager = new(clientDataManager, commandLockoutManager, historyLogProvider, networkProvider, worldProvider);
    private readonly ListFilter<string> _worldNameFilter = new(worldProvider.WorldNames, FilterWorldName);
    
    public void Draw()
    {
        var style = ImGui.GetStyle();
        var shouldProcessSpeakCommand = false;

        SharedUserInterfaces.MediumText("Speak", ImGuiColors.ParsedOrange);
        ImGui.SameLine();
        
        SharedUserInterfaces.MediumText(_speakManager.ChatMode.Beautify());
        ImGui.SameLine();

        SharedUserInterfaces.CommandDescriptionWithQuestionMark(
            description: "Attempts to make selected friend(s) send a message in specified chat mode.",
            requiredPermissions: ["Speak"],
            optionalPermissions: ["Say, Yell, Shout, Tell, Party, Alliance, Free Company, PvP Team", "Linkshell 1 - 8", "Cross-world Linkshell 1 - 8"]
            );

        var friendsMissingPermissions = new List<string>();
        foreach (var target in clientDataManager.TargetManager.Targets)
        {
            if (PermissionChecker.HasValidSpeakPermissions(_speakManager.ChatMode, target.Value.PermissionsGrantedByFriend, _speakManager.LinkshellNumber) == false)
                friendsMissingPermissions.Add(target.Key);
        }

        if (friendsMissingPermissions.Count > 0)
        {
            ImGui.SameLine(ImGui.GetWindowWidth() - ImGui.GetFontSize() - ImGui.GetStyle().WindowPadding.X * 1.5f);
            SharedUserInterfaces.PermissionsWarning(friendsMissingPermissions);
        }

        if (SharedUserInterfaces.IconButton(FontAwesomeIcon.Comment))
            ImGui.OpenPopup("ChatModeSelector");

        SharedUserInterfaces.Tooltip("Change chat channel");

        if (ImGui.BeginPopup("ChatModeSelector"))
        {
            foreach (ChatMode mode in Enum.GetValues(typeof(ChatMode)))
                if (ImGui.Selectable(mode.Beautify(), mode == _speakManager.ChatMode))
                    _speakManager.ChatMode = mode;

            ImGui.EndPopup();
        }

        ImGui.SameLine();
        var additionalArgumentsOffset = ImGui.GetCursorPosX();

        ImGui.SetNextItemWidth(-1 * (SendButtonSize.X + style.WindowPadding.X));
        if (ImGui.InputTextWithHint("###MessageInputBox", "Message", ref _speakManager.Message, 500, ImGuiInputTextFlags.EnterReturnsTrue))
            shouldProcessSpeakCommand = true;

        ImGui.SameLine();

        var isCommandLockout = commandLockoutManager.IsLocked;
        SharedUserInterfaces.DisableIf(isCommandLockout, () =>
        {
            if (ImGui.Button("Send", SendButtonSize))
                shouldProcessSpeakCommand = true;
        });

        switch(_speakManager.ChatMode)
        {
            case ChatMode.Say:
                ImGui.SetCursorPosX(additionalArgumentsOffset);
                ImGui.RadioButton("/say", ref _speakManager.UseEmoteInsteadOfSay, 0);
                SharedUserInterfaces.Tooltip("Executes the message with /say");

                ImGui.SameLine();

                ImGui.RadioButton("/em", ref _speakManager.UseEmoteInsteadOfSay, 1);
                SharedUserInterfaces.Tooltip("Executes the message with /em");
                break;

            case ChatMode.Tell:
                ImGui.SetCursorPosX(additionalArgumentsOffset);
                if (SharedUserInterfaces.IconButton(FontAwesomeIcon.User))
                    _speakManager.SetTellTargetFor(GameObjectManager.GetLocalPlayer());

                SharedUserInterfaces.Tooltip("Copy my name");
                ImGui.SameLine();

                if (SharedUserInterfaces.IconButton(FontAwesomeIcon.Crosshairs))
                    _speakManager.SetTellTargetFor(GameObjectManager.GetTargetPlayer());

                SharedUserInterfaces.Tooltip("Copy my target's name");
                ImGui.SameLine();

                ImGui.SetNextItemWidth(130);
                ImGui.InputTextWithHint("##TellTargetNameInput", "Name", ref _speakManager.TellTargetName, Constraints.PlayerNameCharLimit);
                ImGui.SameLine();

                SharedUserInterfaces.Icon(FontAwesomeIcon.At);
                ImGui.SameLine();

                SharedUserInterfaces.ComboWithFilter("WorldSelector", "World", ref _speakManager.TellTargetWorld, -1, _worldNameFilter);
                break;

            case ChatMode.Linkshell:
            case ChatMode.CrossWorldLinkshell:
                ImGui.SetCursorPosX(additionalArgumentsOffset);
                ImGui.SetCursorPosX(style.WindowPadding.X * 2 + style.FramePadding.X * 2 + ImGui.GetFontSize());

                ImGui.SetNextItemWidth(50);
                if (ImGui.BeginCombo("Linkshell Number", _speakManager.LinkshellNumber.ToString()))
                {
                    for (var i = 1; i < 9; i++)
                    {
                        var selected = i == _speakManager.LinkshellNumber;
                        if (ImGui.Selectable(i.ToString(), selected))
                            _speakManager.LinkshellNumber = i;
                        if (selected)
                            ImGui.SetItemDefaultFocus();
                    }

                    ImGui.EndCombo();
                }

                ImGui.SameLine();
                SharedUserInterfaces.Icon(FontAwesomeIcon.ExclamationCircle);
                if (ImGui.IsItemHovered())
                {
                    ImGui.BeginTooltip();
                    SharedUserInterfaces.TextCentered("Caution!");
                    ImGui.Text("Which number a linkshell corresponds with may differ from person to person.");
                    ImGui.EndTooltip();
                }
                break;
        }

        if (shouldProcessSpeakCommand && isCommandLockout == false)
            _ = _speakManager.SendSpeak();
    }
    
    public void Dispose() => GC.SuppressFinalize(this);
    private static bool FilterWorldName(string worldName, string searchTerm) => worldName.Contains(searchTerm, StringComparison.OrdinalIgnoreCase);
}