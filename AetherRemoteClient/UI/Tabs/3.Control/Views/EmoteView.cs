using System;
using System.Collections.Generic;
using System.Numerics;
using AetherRemoteClient.Domain;
using AetherRemoteClient.Domain.Log;
using AetherRemoteClient.Domain.UI;
using AetherRemoteClient.Providers;
using AetherRemoteClient.UI.Tabs.Managers;
using AetherRemoteClient.UI.Tabs.Modules;
using AetherRemoteCommon.Domain;
using AetherRemoteCommon.Domain.Permissions;
using Dalamud.Interface;
using Dalamud.Interface.Colors;
using ImGuiNET;

namespace AetherRemoteClient.UI.Tabs.Views;

public class EmoteView(
    ClientDataManager clientDataManager,
    CommandLockoutManager commandLockoutManager,
    EmoteProvider emoteProvider,
    HistoryLogProvider historyLogProvider,
    NetworkProvider networkProvider) : IControlTabView
{
    // Const
    private Vector2 _emotePadding = new(4, 4);
    
    // Instantiated
    private readonly EmoteManager _emoteManager = new(clientDataManager, commandLockoutManager, emoteProvider, historyLogProvider, networkProvider);
    private readonly ListFilter<string> _emoteSearchFilter = new(emoteProvider.Emotes, FilterEmote);
    
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
            if (target.Value.PermissionsGrantedByFriend.Primary.HasFlag(PrimaryPermissions.Emote) is false)
                friendsMissingPermissions.Add(target.Key);
        }

        if (friendsMissingPermissions.Count > 0)
        {
            // Hardcoded size of emote selector
            ImGui.SameLine(200 + ImGui.GetStyle().WindowPadding.X * 2.5f);
            SharedUserInterfaces.PermissionsWarning(friendsMissingPermissions);
        }

        ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, _emotePadding);
        SharedUserInterfaces.ComboWithFilter("EmoteSelector", "Emote", ref _emoteManager.Emote, 200, _emoteSearchFilter);
        ImGui.PopStyleVar();

        ImGui.SameLine();

        SharedUserInterfaces.DisableIf(commandLockoutManager.IsLocked, () =>
        {
            if (SharedUserInterfaces.IconButton(FontAwesomeIcon.Play))
                shouldProcessEmoteCommand = true;
        });

        ImGui.Checkbox("Display log message", ref _emoteManager.SendLogMessage);

        if (shouldProcessEmoteCommand)
            _ = _emoteManager.SendEmote();
    }
    
    public void Dispose() => GC.SuppressFinalize(this);

    private static bool FilterEmote(string emote, string searchTerm) => emote.Contains(searchTerm, StringComparison.OrdinalIgnoreCase);

}