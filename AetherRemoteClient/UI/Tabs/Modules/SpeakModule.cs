using AetherRemoteClient.Domain;
using AetherRemoteClient.Domain.Logger;
using AetherRemoteClient.Providers;
using AetherRemoteCommon;
using AetherRemoteCommon.Domain.CommonChatMode;
using Dalamud.Game.ClientState.Objects;
using Dalamud.Interface;
using Dalamud.Interface.Colors;
using Dalamud.Plugin.Services;
using ImGuiNET;
using System;
using System.Numerics;
using System.Threading.Tasks;
using System.Timers;

namespace AetherRemoteClient.UI.Tabs.Modules;

public class SpeakModule : IAetherRemoteModule
{
    // Constants
    private static readonly int SendButtonWidth = 40;

    // Dependencies
    private readonly Configuration configuration;
    private readonly NetworkProvider networkProvider;
    private readonly AetherRemoteLogger logger;
    private readonly IClientState clientState;
    private readonly ITargetManager targetManager;

    // Variables - Speak
    private ChatMode chatMode = ChatMode.Say;
    private int linkshellNumber = 1;
    private string tellTarget = "";
    private string message = "";

    private readonly ControlTargetManager controlTargetManager;
    private readonly Timer commandLockoutTimer;
    private bool lockoutActive = false;

    public SpeakModule(Configuration configuration, NetworkProvider networkProvider, AetherRemoteLogger logger,
        IClientState clientState, ITargetManager targetManager, ControlTargetManager controlTargetManager, Timer commandLockoutTimer)
    {
        this.configuration = configuration;
        this.networkProvider = networkProvider;
        this.logger = logger;
        this.clientState = clientState;
        this.targetManager = targetManager;

        this.controlTargetManager = controlTargetManager;
        this.commandLockoutTimer = commandLockoutTimer;
        this.commandLockoutTimer.Elapsed += EndLockout;
    }

    public void Draw()
    {
        var style = ImGui.GetStyle();
        var shouldProcessSpeakCommand = false;

        var questionIconOffset = SharedUserInterfaces.CalcIconSize(FontAwesomeIcon.QuestionCircle);
        questionIconOffset.Y = 0;
        questionIconOffset.X *= 0.5f;

        SharedUserInterfaces.MediumTextCentered("Speak", null, questionIconOffset);
        ImGui.SameLine();
        ImGui.AlignTextToFramePadding();
        SharedUserInterfaces.Icon(FontAwesomeIcon.QuestionCircle);
        SharedUserInterfaces.Tooltip
            ([
                "Warning! This will actually send messages to the server!", 
                "Force target friends to send a message in specified channel."
            ]);

        SharedUserInterfaces.MediumText("Channel:", ImGuiColors.ParsedOrange);
        ImGui.SameLine();
        SharedUserInterfaces.MediumText(chatMode.ToCondensedString());

        if (SharedUserInterfaces.IconButton(FontAwesomeIcon.Comment))
            ImGui.OpenPopup("ChatModeSelector");

        SharedUserInterfaces.Tooltip("Change chat channel");

        if (ImGui.BeginPopup("ChatModeSelector"))
        {
            foreach (ChatMode mode in Enum.GetValues(typeof(ChatMode)))
                if (ImGui.Selectable(mode.ToCondensedString(), mode == chatMode))
                    chatMode = mode;

            ImGui.EndPopup();
        }

        ImGui.SameLine();

        ImGui.SetNextItemWidth(-1 * (SendButtonWidth + style.WindowPadding.X));
        if (ImGui.InputTextWithHint("###MessageInputBox", "Message", ref message, 500, ImGuiInputTextFlags.EnterReturnsTrue))
            shouldProcessSpeakCommand = true;

        ImGui.SameLine();

        var lockout = lockoutActive;
        if (lockout) ImGui.BeginDisabled();
        if (ImGui.Button("Send", new Vector2(SendButtonWidth, 0)))
            shouldProcessSpeakCommand = true;
        if (lockout) ImGui.EndDisabled();

        if (chatMode == ChatMode.Tell)
        {
            if (SharedUserInterfaces.IconButton(FontAwesomeIcon.User))
                tellTarget = clientState.LocalPlayer?.Name.ToString() ?? tellTarget;

            SharedUserInterfaces.Tooltip("Copy my name");
            ImGui.SameLine();

            if (SharedUserInterfaces.IconButton(FontAwesomeIcon.Crosshairs))
                tellTarget = targetManager.Target?.Name.ToString() ?? tellTarget;

            SharedUserInterfaces.Tooltip("Copy my target's name");
            ImGui.SameLine();

            if (SharedUserInterfaces.IconButton(FontAwesomeIcon.Broom))
                tellTarget = "";

            SharedUserInterfaces.Tooltip("Clear the tell target input field");
            ImGui.SameLine();

            ImGui.SetNextItemWidth(ImGui.GetWindowWidth() / 2);
            ImGui.InputTextWithHint("##TellTargetInput", "Tell Target", ref tellTarget, Constants.PlayerNameCharLimit);
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

        if (shouldProcessSpeakCommand && lockout == false)
        {
            BeginLockout();
            _ = ProcessSpeakCommand();
        }
    }

    private async Task ProcessSpeakCommand()
    {
        if (controlTargetManager.MinimumTargetsMet == false || message.Length <= 0)
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

        var secret = configuration.Secret;
        var targetNames = string.Join(',', controlTargetManager.TargetNames);
        var result = await networkProvider.Speak(secret, controlTargetManager.Targets, message, chatMode, extra);
        if (result.Success)
            logger.LogInternal($"Successfully made {targetNames} say {message} in {chatMode.ToCondensedString()} chat");
        else
            logger.LogInternal($"Unable to make {targetNames} say {message} in {chatMode.ToCondensedString()} chat: {result.Message}");

        // Reset message
        message = "";
    }

    private void BeginLockout()
    {
        commandLockoutTimer.Stop();
        commandLockoutTimer.Start();

        lockoutActive = true;
    }

    private void EndLockout(object? sender, ElapsedEventArgs e)
    {
        lockoutActive = false;
    }

    public void Dispose()
    {
        commandLockoutTimer.Elapsed -= EndLockout;
        GC.SuppressFinalize(this);
    }
}
