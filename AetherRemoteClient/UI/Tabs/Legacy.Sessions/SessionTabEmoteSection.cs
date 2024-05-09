using AetherRemoteClient.Domain;
using AetherRemoteClient.Providers;
using Dalamud.Interface;
using Dalamud.Interface.Colors;
using ImGuiNET;
using System;
using System.Text;
using System.Threading.Tasks;

namespace AetherRemoteClient.UI.Experimental.Tabs.Sessions.Emote;

public class SessionTabEmoteSection(NetworkProvider networkProvider, EmoteProvider emoteProvider)
{
    // Injected
    private readonly NetworkProvider networkProvider = networkProvider;
    private readonly EmoteProvider emoteProvider = emoteProvider;

    // Local
    private string emote = "";
    private Session? currentSession = null;

    //private readonly ThreadedFilter<string> emoteFilter = new(emoteProvider.Emotes, (emote, searchTerm) => { return emote.Contains(searchTerm); });

    public void SetSession(Session newSession)
    {
        currentSession = newSession;
    }

    public void DrawEmoteSection()
    {
        var shouldProcessEmoteCommand = false;

        SharedUserInterfaces.MediumText("Emote", ImGuiColors.ParsedOrange);

        // Deprecated - Legacy Code
        // SharedUserInterfaces.ComboFilter("###EmoteSelector", ref emote, emoteFilter);

        ImGui.SameLine();

        /* Deprecated - Legacy Code
        if (SharedUserInterfaces.IconButtonScaled(FontAwesomeIcon.Play))
        {
            shouldProcessEmoteCommand = true;
        }
        */

        if (shouldProcessEmoteCommand)
        {
            _ = ProcessEmoteCommand();
        }
    }

    private async Task ProcessEmoteCommand()
    {
        if (currentSession == null)
            return;

        var validEmote = emoteProvider.Emotes.Contains(emote);
        if (validEmote == false)
            return;

        var secret = ""; // secretProvider.Secret;
        var targets = currentSession.TargetFriends;
        var result = await networkProvider.Emote(secret, targets, emote);
        if (result.Success)
        {
            var sb = new StringBuilder();
            sb.Append("You made ");
            sb.Append(currentSession.TargetFriendsAsList());
            sb.Append(" do the ");
            sb.Append(emote);
            sb.Append(" emote.");

            AetherRemoteLogging.Log("Me", sb.ToString(), DateTime.Now, LogType.Sent);
        }
    }
}
