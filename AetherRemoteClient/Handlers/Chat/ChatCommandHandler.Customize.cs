using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AetherRemoteClient.Handlers.Chat;

public partial class ChatCommandHandler
{
    /// <summary>
    ///     Handle an emote slash command
    /// </summary>
    private async Task HandleCustomize(string args)
    {
        // /ar customize | friendCode1, friendCode2 | profile name | optional: merge
        var arguments = args.Split('|', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        
        // If the number of arguments is less than the amount required
        if (arguments.Length < 3)
        {
            SendChatMessage("Invalid number of arguments");
            return;
        }
        
        // Extract arguments
        var argsTargets = arguments[1];
        var argsProfileName = arguments[2];
        
        // Format Targets
        var targets = argsTargets.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        // Try to get the profile. Chat messages are handled internally by this function, so we can just return
        if (await TryGetProfileByProfileName(argsProfileName).ConfigureAwait(false) is not { } profileAsBytes)
            return;
        
        // Grab the "should merge / additive" thing as well
        var shouldApplyAsAdditive = false;
        if (arguments.Length == 4)
            shouldApplyAsAdditive = bool.TryParse(arguments[3], out var value) && value;
     
        // Send
        await _networkCommandManager.SendCustomize(targets.ToList(), profileAsBytes, shouldApplyAsAdditive).ConfigureAwait(false);
    }

    private async Task<byte[]?> TryGetProfileByProfileName(string profileName)
    {
        // Format Profile
        var profiles = await _customizePlusService.GetProfilesPlain().ConfigureAwait(false);
        
        // Check to see if the profile name is one of our profiles
        var profileId = Guid.Empty;
        foreach (var profile in profiles)
        {
            if (profile.Name.Equals(profileName) is false)
                continue;

            profileId = profile.Guid;
            break;
        }
        
        // If we didn't find an id, it doesn't exist
        if (profileId == Guid.Empty)
        {
            SendChatMessage("Profile not found, please check spelling and case-sensitivity");
            return null;
        }
        
        // Get the profile
        if (await _customizePlusService.GetProfile(profileId).ConfigureAwait(false) is not { } raw)
        {
            SendChatMessage("Internal error occurred, please type /xllog to learn more");
            return null;
        }

        try
        {
            // Return the encoded bytes
            return Encoding.UTF8.GetBytes(raw);
        }
        catch (Exception e)
        {
            Plugin.Log.Error($"[ChatCommandHandler.Customize.TryGetProfileByProfileName] {e}");
            return null;
        }
    }
}