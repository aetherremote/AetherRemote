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
        var emotes = Plugin.DataManager.Excel.GetSheet<Emote>();
        for (uint i = 0; i < emotes.Count; i++)
        {
            var emote = emotes.GetRowOrDefault(i);
            if (emote is null) continue;

            var command = emote.GetValueOrDefault().TextCommand.ValueNullable?.Command.ToString();
            if (command.IsNullOrEmpty()) continue;
            var commandWithoutSlash = command[1..];
            
            /*
             * For some reason, there is a bug in game where if someone is actively doing the /pet emote
             * and someone in Aether Remote issues them a /stroke command, the game will hard crash to
             * desktop without any crash logs. So hopefully, just replacing /stroke with /pet will
             * prevent a situation where this can happen.
             */
            Emotes.Add(commandWithoutSlash == "stroke" ? "pet" : commandWithoutSlash);

            var shortCommand = emote.GetValueOrDefault().TextCommand.ValueNullable?.ShortCommand.ExtractText();
            if (shortCommand.IsNullOrEmpty()) continue;
            var shortCommandWithoutSlash = shortCommand[1..];
            Emotes.Add(shortCommandWithoutSlash == "stroke" ? "pet" : shortCommandWithoutSlash);
        }

        Emotes.Sort();
    }
}