using AetherRemoteClient.Domain;
using AetherRemoteClient.Domain.Logger;
using AetherRemoteClient.Providers;
using AetherRemoteCommon.Domain.CommonChatMode;
using Dalamud.Interface;
using Dalamud.Interface.Colors;
using ImGuiNET;
using System;
using System.Numerics;
using System.Threading.Tasks;
using System.Timers;

namespace AetherRemoteClient.UI.Tabs.Modules;

public class EmoteModule : IAetherRemoteModule
{
    // Dependencies
    private readonly Configuration configuration;
    private readonly EmoteProvider emoteProvider;
    private readonly NetworkProvider networkProvider;
    private readonly AetherRemoteLogger logger;

    // Variables - Emote
    private string emote = "";
    private readonly ListFilter<string> emoteSearchFilter;

    private readonly ControlTargetManager controlTargetManager;
    private readonly Timer commandLockoutTimer;
    private bool lockoutActive = false;

    public EmoteModule(Configuration configuration, EmoteProvider emoteProvider, NetworkProvider networkProvider, 
        AetherRemoteLogger logger, ControlTargetManager controlTargetManager, Timer commandLockoutTimer)
    {
        this.configuration = configuration;
        this.emoteProvider = emoteProvider;
        this.networkProvider = networkProvider;
        this.logger = logger;

        this.controlTargetManager = controlTargetManager;
        this.commandLockoutTimer = commandLockoutTimer;
        this.commandLockoutTimer.Elapsed += EndLockout;

        emoteSearchFilter = new(emoteProvider.Emotes, FilterEmote);
    }

    public void Draw()
    {
        var shouldProcessEmoteCommand = false;

        var questionIconOffset = SharedUserInterfaces.CalcIconSize(FontAwesomeIcon.QuestionCircle);
        questionIconOffset.Y = 0;
        questionIconOffset.X *= 0.5f;

        SharedUserInterfaces.MediumTextCentered("Emote", null, questionIconOffset);
        ImGui.SameLine();
        ImGui.AlignTextToFramePadding();
        SharedUserInterfaces.Icon(FontAwesomeIcon.QuestionCircle);
        SharedUserInterfaces.Tooltip("Force target friends to preform an emote.");

        SharedUserInterfaces.MediumText("Emote", ImGuiColors.ParsedOrange);

        ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new Vector2(4, 4));
        SharedUserInterfaces.ComboWithFilter(ref emote, "Emote", emoteSearchFilter);
        ImGui.PopStyleVar();

        ImGui.SameLine();

        var lockout = lockoutActive;
        if (lockout) ImGui.BeginDisabled();
        if (SharedUserInterfaces.IconButton(FontAwesomeIcon.Play))
            shouldProcessEmoteCommand = true;
        if (lockout) ImGui.EndDisabled();

        if (shouldProcessEmoteCommand && lockout == false)
        {
            BeginLockout();
            _ = ProcessEmoteCommand();
        }
    }

    private async Task ProcessEmoteCommand()
    {
        if (controlTargetManager.MinimumTargetsMet == false || emote.Length <= 0)
            return;

        var validEmote = emoteProvider.Emotes.Contains(emote);
        if (validEmote == false)
            return;

        var secret = configuration.Secret;
        var targetNames = string.Join(',', controlTargetManager.TargetNames);
        var result = await networkProvider.IssueEmoteCommand(secret, controlTargetManager.Targets, emote);
        logger.LogInternal($"{(result ? "Successfully made" : "Unable to make")} {targetNames} do the {emote} emote");

        // Reset emote
        emote = "";
        emoteSearchFilter.UpdateSearchTerm("");
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

    public static bool FilterEmote(string emote, string searchTerm)
    {
        return emote.Contains(searchTerm);
    }
}
