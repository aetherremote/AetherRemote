using System;
using System.Linq;
using System.Threading.Tasks;
using AetherRemoteCommon.Domain.Enums;

namespace AetherRemoteClient.Handlers.Chat;

public partial class ChatCommandHandler
{
    /// <summary>
    ///     Handle an emote slash command
    /// </summary>
    private async Task HandleTransform(string args)
    {
        // /ar transform | friendCode1, friendCode2 | design name | optional: apply type
        var arguments = args.Split('|', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        
        // If the number of arguments is less than the amount required
        if (arguments.Length < 3)
        {
            SendChatMessage("Invalid number of arguments");
            return;
        }
        
        // Extract arguments
        var argsTargets = arguments[1];
        var argsDesignName = arguments[2];
        
        // Format Targets
        var targets = argsTargets.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        
        // Try to get the profile. Chat messages are handled internally by this function, so we can just return
        if (await TryGetDesignByName(argsDesignName).ConfigureAwait(false) is not { } design)
            return;

        // Get the apply type if it exists, or otherwise just apply it all
        var applyType = arguments.Length < 4
            ? GlamourerApplyFlags.All | GlamourerApplyFlags.Once
            : arguments[3] switch
            {
                "equipment" => GlamourerApplyFlags.Equipment | GlamourerApplyFlags.Once,
                "customization" => GlamourerApplyFlags.Customization | GlamourerApplyFlags.Once,
                _ => GlamourerApplyFlags.All | GlamourerApplyFlags.Once
            };

        // Send
        await _networkCommandManager.SendTransformation(targets.ToList(), design, applyType).ConfigureAwait(false);
    }
    
    private async Task<string?> TryGetDesignByName(string designName)
    {
        if (await _glamourerService.GetDesignList().ConfigureAwait(false) is not { } designs)
        {
            SendChatMessage("Could not get designs list, type /xllog to learn more");
            return null;
        }
        
        // Check to see if the profile name is one of our profiles
        var designId = Guid.Empty;
        foreach (var designEntry in designs)
        {
            if (designEntry.Name.Equals(designName) is false)
                continue;

            designId = designEntry.Id;
            break;
        }
        
        // If we didn't find an id, it doesn't exist
        if (designId == Guid.Empty)
        {
            SendChatMessage("Design not found, please check spelling and case-sensitivity");
            return null;
        }
        
        // Get the design
        if (await _glamourerService.GetDesignAsync(designId).ConfigureAwait(false) is not { } design)
        {
            SendChatMessage("Internal error occurred, please type /xllog to learn more");
            return null;
        }

        return design;
    }
}