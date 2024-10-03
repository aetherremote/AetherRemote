using Dalamud.Utility;
using Lumina.Excel.GeneratedSheets;
using System.Collections.Generic;

namespace AetherRemoteClient.Providers;

/// <summary>
/// Provides a list all current available emotes in game
/// </summary>
public class EmoteProvider
{
    /// <summary>
    /// List containing all the current emote alias in the game.
    /// </summary>
    public List<string> Emotes { get; private set; }

    /// <summary>
    /// <inheritdoc cref="EmoteProvider"/>
    /// </summary>
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

    /// <summary>
    /// Checks if the provided emote is valid
    /// </summary>
    public bool ValidEmote(string emote) => Emotes.Contains(emote);
}
