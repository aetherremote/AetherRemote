using AetherRemoteClient.Domain;
using AetherRemoteClient.Domain.Log;
using AetherRemoteClient.Domain.UI;
using AetherRemoteClient.Providers;
using AetherRemoteCommon;
using AetherRemoteCommon.Domain;
using AetherRemoteCommon.Domain.CommonChatMode;
using AetherRemoteCommon.Domain.Network.Commands;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Interface;
using Dalamud.Interface.Colors;
using FFXIVClientStructs.FFXIV.Client.Game.Character;
using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;

namespace AetherRemoteClient.UI.Tabs.Modules;

public class SpeakModule : IControlTableModule
{
    // Const
    private static readonly Vector2 SendButtonSize = new(40, 0);

    // Injected
    private readonly ClientDataManager clientDataManager;
    private readonly CommandLockoutManager commandLockoutManager;
    private readonly HistoryLogManager historyLogManager;
    private readonly NetworkProvider networkProvider;
    private readonly WorldProvider worldProvider;

    // Instantiated
    private readonly ListFilter<string> worldNameFilter;

    // Variables - Speak
    private ChatMode chatMode = ChatMode.Say;
    private int linkshellNumber = 1;
    private string message = "";
    private int userEmoteInsteadOfSay = 0;

    private string tellTargetName = "";
    private string tellTargetWorld = "";

    public SpeakModule(
        ClientDataManager clientDataManager,
        CommandLockoutManager commandLockoutManager,
        HistoryLogManager historyLogManager,
        NetworkProvider networkProvider,
        WorldProvider worldProvider)
    {
        this.clientDataManager = clientDataManager;
        this.commandLockoutManager = commandLockoutManager;
        this.historyLogManager = historyLogManager;
        this.networkProvider = networkProvider;
        this.worldProvider = worldProvider;

        worldNameFilter = new(worldProvider.WorldNames, FilterWorldName);
    }

    public void Draw()
    {
        var style = ImGui.GetStyle();
        var shouldProcessSpeakCommand = false;

        SharedUserInterfaces.MediumText("Speak", ImGuiColors.ParsedOrange);
        ImGui.SameLine();
        SharedUserInterfaces.MediumText(chatMode.Beautify());
        ImGui.SameLine();

        SharedUserInterfaces.CommandDescriptionWithQuestionMark(
            description: "Attempts to make selected friend(s) send a message in specified chat mode.",
            requiredPermissions: ["Speak"],
            optionalPermissions: ["Say, Yell, Shout, Tell, Party, Alliance, Free Company, PvP Team", "Linkshell 1 - 8", "Crossworld Linkshell 1 - 8"]
            );

        var friendsMissingPermissions = new List<string>();
        foreach (var target in clientDataManager.TargetManager.Targets)
        {
            if (PermissionChecker.HasValidSpeakPermissions(chatMode, target.Value.PermissionsGrantedByFriend, linkshellNumber) == false)
                friendsMissingPermissions.Add(target.Key);
        }

        if (friendsMissingPermissions.Count > 0)
        {
            ImGui.SameLine(ImGui.GetWindowWidth() - ImGui.GetFontSize() - (ImGui.GetStyle().WindowPadding.X * 1.5f));
            SharedUserInterfaces.PermissionsWarning(friendsMissingPermissions);
        }

        if (SharedUserInterfaces.IconButton(FontAwesomeIcon.Comment))
            ImGui.OpenPopup("ChatModeSelector");

        SharedUserInterfaces.Tooltip("Change chat channel");

        if (ImGui.BeginPopup("ChatModeSelector"))
        {
            foreach (ChatMode mode in Enum.GetValues(typeof(ChatMode)))
                if (ImGui.Selectable(mode.Beautify(), mode == chatMode))
                    chatMode = mode;

            ImGui.EndPopup();
        }

        ImGui.SameLine();
        var additionalArgumentsOffset = ImGui.GetCursorPosX();

        ImGui.SetNextItemWidth(-1 * (SendButtonSize.X + style.WindowPadding.X));
        if (ImGui.InputTextWithHint("###MessageInputBox", "Message", ref message, 500, ImGuiInputTextFlags.EnterReturnsTrue))
            shouldProcessSpeakCommand = true;

        ImGui.SameLine();

        var isCommandLockout = commandLockoutManager.IsLocked;
        SharedUserInterfaces.DisableIf(isCommandLockout, () =>
        {
            if (ImGui.Button("Send", SendButtonSize))
                shouldProcessSpeakCommand = true;
        });

        switch(chatMode)
        {
            case ChatMode.Say:
                ImGui.SetCursorPosX(additionalArgumentsOffset);
                ImGui.RadioButton("/say", ref userEmoteInsteadOfSay, 0);
                SharedUserInterfaces.Tooltip("Executes the message with /say");

                ImGui.SameLine();

                ImGui.RadioButton("/em", ref userEmoteInsteadOfSay, 1);
                SharedUserInterfaces.Tooltip("Executes the message with /em");
                break;

            case ChatMode.Tell:
                ImGui.SetCursorPosX(additionalArgumentsOffset);
                if (SharedUserInterfaces.IconButton(FontAwesomeIcon.User))
                    SetTellTargetFor(Plugin.ClientState.LocalPlayer);

                SharedUserInterfaces.Tooltip("Copy my name");
                ImGui.SameLine();

                if (SharedUserInterfaces.IconButton(FontAwesomeIcon.Crosshairs))
                    SetTellTargetFor(Plugin.TargetManager.Target);

                SharedUserInterfaces.Tooltip("Copy my target's name");
                ImGui.SameLine();

                ImGui.SetNextItemWidth(130);
                ImGui.InputTextWithHint("##TellTargetNameInput", "Name", ref tellTargetName, Constraints.PlayerNameCharLimit);
                ImGui.SameLine();

                SharedUserInterfaces.Icon(FontAwesomeIcon.At);
                ImGui.SameLine();

                SharedUserInterfaces.ComboWithFilter("WorldSelector", "World", ref tellTargetWorld, -1, worldNameFilter);
                break;

            case ChatMode.Linkshell:
            case ChatMode.CrossworldLinkshell:
                ImGui.SetCursorPosX(additionalArgumentsOffset);
                ImGui.SetCursorPosX((style.WindowPadding.X * 2) + (style.FramePadding.X * 2) + ImGui.GetFontSize());

                ImGui.SetNextItemWidth(50);
                if (ImGui.BeginCombo("Linkshell Number", linkshellNumber.ToString()))
                {
                    for (var i = 1; i < 9; i++)
                    {
                        var selected = i == linkshellNumber;
                        if (ImGui.Selectable(i.ToString(), selected))
                            linkshellNumber = i;
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
            _ = ProcessSpeakCommand();
    }

    private async Task ProcessSpeakCommand()
    {
        if (clientDataManager.TargetManager.Targets.Count > Constraints.MaximumTargetsForInGameOperations)
            return;

        if (string.IsNullOrEmpty(message))
            return;

        var extra = chatMode switch
        {
            ChatMode.Linkshell or ChatMode.CrossworldLinkshell => linkshellNumber.ToString(),
            ChatMode.Tell => $"{tellTargetName}@{tellTargetWorld}",
            ChatMode.Say => userEmoteInsteadOfSay.ToString(),
            _ => null
        };

        // Initiate UI Lockout
        commandLockoutManager.Lock(Constraints.GameCommandCooldownInSeconds);

        var targets = clientDataManager.TargetManager.Targets.Keys.ToList();
        var result = await IssueSpeakCommand(targets, message, chatMode, extra).ConfigureAwait(false);
        if (result)
        {
            var targetNames = string.Join(", ", targets);
            var logMessage = chatMode switch
            {
                ChatMode.Linkshell => $"You issued {targetNames} to say \"{message}\" in LS{extra}.",
                ChatMode.CrossworldLinkshell => $"You issued {targetNames} to say \"{message}\" in CWL{extra}.",
                ChatMode.Tell => $"You issued {targetNames} to say \"{message}\" in a tell to {extra}",
                _ => $"You issued {targetNames} to say \"{message}\" in {chatMode.Beautify()} chat",
            };

            Plugin.Log.Information(logMessage);
            historyLogManager.LogHistory(logMessage);

            // Reset message
            message = "";
        }
        else
        {
            // TODO: Make a toast with what went wrong
        }
    }

    private unsafe void SetTellTargetFor(IGameObject? target)
    {
        if (target is null) return;

        var character = CharacterManager.Instance()->LookupBattleCharaByEntityId(target.EntityId);
        if (character is null) return;

        var homeWorldId = character->HomeWorld;
        var homeWorld = worldProvider.TryGetWorldById(homeWorldId);
        if (homeWorld is null) return;

        tellTargetName = character->NameString ?? tellTargetName;
        tellTargetWorld = homeWorld;
    }

    public async Task<bool> IssueSpeakCommand(List<string> targets, string message, ChatMode chatMode, string? extra)
    {
        #pragma warning disable CS0162
        if (Plugin.DeveloperMode)
            return true;
        #pragma warning restore CS0162

        var request = new SpeakRequest(targets, message, chatMode, extra);
        var result = await networkProvider.InvokeCommand<SpeakRequest, SpeakResponse>(Network.Commands.Speak, request);
        if (result.Success == false)
            Plugin.Log.Warning($"Issuing speak command unsuccessful: {result.Message}");

        return result.Success;
    }

    public void Dispose() => GC.SuppressFinalize(this);
    private static bool FilterWorldName(string worldName, string searchTerm) => worldName.Contains(searchTerm);
}
