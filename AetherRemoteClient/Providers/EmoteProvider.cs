using Dalamud.Utility;
using System.Collections.Generic;
using Lumina.Excel.Sheets;

namespace AetherRemoteClient.Providers;

/// <summary>
/// Provides a list all current available emotes in game
/// </summary>
public class EmoteProvider
{
    /// <summary>
    /// List containing all the current emote alias in the game.
    /// </summary>
    public List<string> Emotes { get; } = [];

    /// <summary>
    /// <inheritdoc cref="EmoteProvider"/>
    /// </summary>
    public EmoteProvider()
    {
        var emoteSheet = Plugin.DataManager.Excel.GetSheet<Emote>();
        
        for (uint i = 0; i < emoteSheet.Count; i++)
        {
            var emote = emoteSheet.GetRowOrDefault(i);
            if (emote is null) continue;
            
            var command = emote.GetValueOrDefault().TextCommand.ValueNullable?.Command.ToString();
            if (command.IsNullOrEmpty()) continue;
            Emotes.Add(command[1..]);

            var shortCommand = emote.GetValueOrDefault().TextCommand.ValueNullable?.ShortCommand.ExtractText();
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
