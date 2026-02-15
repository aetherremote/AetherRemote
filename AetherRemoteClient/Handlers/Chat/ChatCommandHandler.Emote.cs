using System;
using System.Linq;
using System.Threading.Tasks;

namespace AetherRemoteClient.Handlers.Chat;

public partial class ChatCommandHandler
{
    /// <summary>
    ///     Handle an emote slash command
    /// </summary>
    private async Task HandleEmote(string args)
    {
        // /ar emote | friendCode1, friendCode2 | emote | optional: displayLogMessage
        var arguments = args.Split('|', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        
        // If the number of arguments is less than the amount required
        if (arguments.Length < 3)
        {
            SendChatMessage("Invalid number of arguments");
            return;
        }
        
        // Extract arguments
        var argsTargets = arguments[1];
        var argsEmoteName = arguments[2];

        // Format Targets
        var targets = argsTargets.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        
        // Validate emote is real
        if (_emoteService.Emotes.Contains(argsEmoteName) is false)
        {
            SendChatMessage("Unknown emote");
            return;
        }
        
        // Grab the "include log" thing as well
        var displayLogMessage = false;
        if (arguments.Length == 4)
            displayLogMessage = bool.TryParse(arguments[3], out var value) && value;

        // Send
        await _networkCommandManager.SendEmote(targets.ToList(), argsEmoteName, displayLogMessage).ConfigureAwait(false);
    }
}