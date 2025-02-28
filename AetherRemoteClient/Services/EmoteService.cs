using System.Collections.Generic;
using Dalamud.Utility;
using Lumina.Excel.Sheets;

namespace AetherRemoteClient.Services;

/// <summary>
///     Provides a list of available emotes in game
/// </summary>
public class EmoteService
{
    /// <summary>
    ///     List of available emoting in game
    /// </summary>
    public readonly List<string> Emotes = [];

    /// <summary>
    ///     <inheritdoc cref="EmoteService" />
    /// </summary>
    public EmoteService()
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
}