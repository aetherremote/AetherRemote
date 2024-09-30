using Dalamud.Utility;
using Lumina.Excel.GeneratedSheets;
using System.Collections.Generic;

namespace AetherRemoteClient.Providers;

public class EmoteProvider
{
    /// <summary>
    /// List containing all the current emote alias in the game.
    /// </summary>
    public List<string> Emotes { get; private set; }

    public EmoteProvider()
    {
        Emotes = [];
        var emoteSheet = Plugin.DataManager.Excel.GetSheet<Emote>();
        if (emoteSheet == null) return;

        for (uint i = 0; i < emoteSheet.RowCount; i++)
        {
            var emote = emoteSheet.GetRow(i);
            if (emote == null) continue;

            var command = emote?.TextCommand?.Value?.Command?.ToString();
            if (command.IsNullOrEmpty()) continue;
            Emotes.Add(command[1..]);

            var shortCommand = emote?.TextCommand?.Value?.ShortCommand?.ToString();
            if (shortCommand.IsNullOrEmpty()) continue;
            Emotes.Add(shortCommand[1..]);
        }

        Emotes.Sort();
    }

    public bool ValidEmote(string emote) => Emotes.Contains(emote);
}
