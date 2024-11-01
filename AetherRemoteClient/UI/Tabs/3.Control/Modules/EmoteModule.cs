using AetherRemoteClient.Domain;
using AetherRemoteClient.Domain.Log;
using AetherRemoteClient.Domain.UI;
using AetherRemoteClient.Providers;
using AetherRemoteCommon;
using AetherRemoteCommon.Domain;
using AetherRemoteCommon.Domain.Network.Commands;
using Dalamud.Interface;
using Dalamud.Interface.Colors;
using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;

namespace AetherRemoteClient.UI.Tabs.Modules;

public class EmoteModule : IControlTableModule
{
    // Injected
    private readonly ClientDataManager clientDataManager;
    private readonly CommandLockoutManager commandLockoutManager;
    private readonly EmoteProvider emoteProvider;
    private readonly HistoryLogManager historyLogManager;
    private readonly NetworkProvider networkProvider;

    // Variables - Emote
    private string emote = "";
    private bool sendLogMessage = false;
    private readonly ListFilter<string> emoteSearchFilter;

    public EmoteModule(
        ClientDataManager clientDataManager,
        CommandLockoutManager commandLockoutManager,
        EmoteProvider emoteProvider,
        HistoryLogManager historyLogManager,
        NetworkProvider networkProvider)
    {
        this.clientDataManager = clientDataManager;
        this.commandLockoutManager = commandLockoutManager;
        this.emoteProvider = emoteProvider;
        this.historyLogManager = historyLogManager;
        this.networkProvider = networkProvider;

        emoteSearchFilter = new(emoteProvider.Emotes, FilterEmote);
    }

    public void Draw()
    {
        var shouldProcessEmoteCommand = false;

        SharedUserInterfaces.MediumText("Emote", ImGuiColors.ParsedOrange);
        ImGui.SameLine();

        SharedUserInterfaces.CommandDescriptionWithQuestionMark(
            description: "Attempts to make selected friend(s) perform specified emote.",
            requiredPermissions: ["Emote"]);

        var friendsMissingPermissions = new List<string>();
        foreach (var target in clientDataManager.TargetManager.Targets)
        {
            if (PermissionChecker.HasValidEmotePermissions(target.Value.PermissionsGrantedByFriend) == false)
                friendsMissingPermissions.Add(target.Key);
        }

        if (friendsMissingPermissions.Count > 0)
        {
            // Hardcoded size of emote selector
            ImGui.SameLine(200 + (ImGui.GetStyle().WindowPadding.X * 2.5f));
            SharedUserInterfaces.PermissionsWarning(friendsMissingPermissions);
        }

        ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new Vector2(4, 4));
        SharedUserInterfaces.ComboWithFilter("EmoteSelector", "Emote", ref emote, 200, emoteSearchFilter);
        ImGui.PopStyleVar();

        ImGui.SameLine();

        SharedUserInterfaces.DisableIf(commandLockoutManager.IsLocked, () =>
        {
            if (SharedUserInterfaces.IconButton(FontAwesomeIcon.Play))
                shouldProcessEmoteCommand = true;
        });

        ImGui.Checkbox("Display log message", ref sendLogMessage);

        if (shouldProcessEmoteCommand)
            _ = ProcessEmoteCommand();
    }

    private async Task ProcessEmoteCommand()
    {
        if (clientDataManager.TargetManager.Targets.Count > Constraints.MaximumTargetsForInGameOperations) return;
        if ((emote.Length > 0 && emoteProvider.ValidEmote(emote)) == false) return;

        // Initiate UI Lockout
        commandLockoutManager.Lock(Constraints.GameCommandCooldownInSeconds);

        var targets = clientDataManager.TargetManager.Targets.Keys.ToList();
        var result = await IssueEmoteCommand(targets, emote, sendLogMessage).ConfigureAwait(false);
        if (result)
        {
            var message = $"You issued {string.Join(", ", targets)} to do the {emote} emote";
            Plugin.Log.Information(message);
            historyLogManager.LogHistory(message);

            // Reset emote
            emote = string.Empty;
            emoteSearchFilter.UpdateSearchTerm(string.Empty);
        }
        else
        {
            // TODO: Make a toast with what went wrong
        }
    }

    public async Task<bool> IssueEmoteCommand(List<string> targets, string emote, bool sendMessageLog)
    {
        #pragma warning disable CS0162
        if (Plugin.DeveloperMode)
            return true;
        #pragma warning restore CS0162

        var request = new EmoteRequest(targets, emote, sendMessageLog);
        var result = await networkProvider.InvokeCommand<EmoteRequest, EmoteResponse>(Network.Commands.Emote, request);
        if (result.Success == false)
            Plugin.Log.Warning($"Issuing emote command unsuccessful: {result.Message}");

        return result.Success;
    }

    public void Dispose() => GC.SuppressFinalize(this);
    private static bool FilterEmote(string emote, string searchTerm) => emote.Contains(searchTerm, StringComparison.OrdinalIgnoreCase);
}
