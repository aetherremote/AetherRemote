using AetherRemoteClient.Domain;
using AetherRemoteClient.Domain.Log;
using AetherRemoteClient.Domain.UI;
using AetherRemoteClient.Providers;
using AetherRemoteCommon;
using Dalamud.Interface;
using Dalamud.Interface.Colors;
using ImGuiNET;
using System;
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
        ImGui.AlignTextToFramePadding();
        SharedUserInterfaces.Icon(FontAwesomeIcon.QuestionCircle);

        if (ImGui.IsItemHovered())
        {
            ImGui.BeginTooltip();
            ImGui.TextColored(ImGuiColors.ParsedOrange, "[Description]");
            ImGui.Text("Attempts to make selected friend(s) perform specified emote.");
            
            ImGui.Separator();

            ImGui.TextColored(ImGuiColors.ParsedOrange, "[Required Permissions]");
            ImGui.BulletText("Emote");
            ImGui.EndTooltip();
        }

        ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new Vector2(4, 4));
        SharedUserInterfaces.ComboWithFilter(ref emote, "Emote", emoteSearchFilter);
        ImGui.PopStyleVar();

        ImGui.SameLine();

        SharedUserInterfaces.DisableIf(commandLockoutManager.IsLocked, () =>
        {
            if (SharedUserInterfaces.IconButton(FontAwesomeIcon.Play))
                shouldProcessEmoteCommand = true;
        });

        if (shouldProcessEmoteCommand)
            _ = ProcessEmoteCommand();
    }

    private async Task ProcessEmoteCommand()
    {
        if (clientDataManager.TargetManager.Targets.Count > Constraints.MaximumTargetsForInGameOperations)
            return;

        if ((emote.Length > 0 && emoteProvider.ValidEmote(emote)) == false)
            return;

        // Initiate UI Lockout
        commandLockoutManager.Lock(Constraints.GameCommandCooldownInSeconds);

        var targets = clientDataManager.TargetManager.Targets.ToList();
        var result = await networkProvider.IssueEmoteCommand(targets, emote).ConfigureAwait(false);
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

    public void Dispose() => GC.SuppressFinalize(this);
    private static bool FilterEmote(string emote, string searchTerm) => emote.Contains(searchTerm);
}
