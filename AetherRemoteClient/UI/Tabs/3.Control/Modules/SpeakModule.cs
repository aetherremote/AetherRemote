using AetherRemoteClient.Domain;
using AetherRemoteClient.Domain.Log;
using AetherRemoteClient.Domain.UI;
using AetherRemoteClient.Providers;
using AetherRemoteCommon;
using AetherRemoteCommon.Domain.CommonChatMode;
using AetherRemoteCommon.Domain.Network.Commands;
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

    // Variables - Speak
    private ChatMode chatMode = ChatMode.Say;
    private int linkshellNumber = 1;
    private string tellTarget = "";
    private string message = "";

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

        if (chatMode == ChatMode.Tell)
        {
            if (SharedUserInterfaces.IconButton(FontAwesomeIcon.User))
                tellTarget = Plugin.ClientState.LocalPlayer?.Name.ToString() ?? tellTarget;

            SharedUserInterfaces.Tooltip("Copy my name");
            ImGui.SameLine();

            if (SharedUserInterfaces.IconButton(FontAwesomeIcon.Crosshairs))
                tellTarget = GetTellTarget() ?? tellTarget;

            SharedUserInterfaces.Tooltip("Copy my target's name");
            ImGui.SameLine();

            if (SharedUserInterfaces.IconButton(FontAwesomeIcon.Broom))
                tellTarget = string.Empty;

            SharedUserInterfaces.Tooltip("Clear the tell target input field");
            ImGui.SameLine();

            ImGui.SetNextItemWidth(ImGui.GetWindowWidth() / 2);
            ImGui.InputTextWithHint("##TellTargetInput", "Tell Target", ref tellTarget, Constraints.TellTargetLimit);
        }
        else if (chatMode == ChatMode.Linkshell || chatMode == ChatMode.CrossworldLinkshell)
        {
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

        string? extra = null;
        if (chatMode == ChatMode.Linkshell || chatMode == ChatMode.CrossworldLinkshell)
        {
            extra = linkshellNumber.ToString();
        }
        else if (chatMode == ChatMode.Tell)
        {
            if (tellTarget.Length > 0)
                extra = tellTarget;
        }

        // Initiate UI Lockout
        commandLockoutManager.Lock(Constraints.GameCommandCooldownInSeconds);

        var targets = clientDataManager.TargetManager.Targets.ToList();
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

    private unsafe string? GetTellTarget()
    {
        var targetName = Plugin.TargetManager.Target?.Name.ToString();
        if (targetName == null)
            return null;

        // Can we look this up by id rather than name?
        var characterObject = CharacterManager.Instance() -> LookupBattleCharaByName(targetName, true);
        if (characterObject is null)
            return null;

        var worldId = characterObject->CurrentWorld;
        var worldName = worldProvider.TryGetWorld(worldId);
        if (worldName == null)
            return null;

        return $"{targetName}@{worldName}";
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
}
