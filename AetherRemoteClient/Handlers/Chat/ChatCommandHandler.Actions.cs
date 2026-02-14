using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AetherRemoteClient.Style;
using AetherRemoteCommon.Domain.Enums;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Game.Text.SeStringHandling.Payloads;

namespace AetherRemoteClient.Handlers.Chat;

public partial class ChatCommandHandler
{
    /// <summary>
    ///     Handle an emote slash command
    /// </summary>
    private async Task HandleCustomize(string args)
    {
        // Maybe this is better to do *before* passing it in?
        var parsed = ExtractArguments(args);
        if (parsed.Length < 3)
        {
            var payloads = new List<Payload>
            {
                new UIForegroundPayload(AetherRemoteColors.TextColorPurple),
                new TextPayload("[AetherRemote] "),
                UIForegroundPayload.UIForegroundOff,
                new TextPayload("Invalid syntax. Example /ar customize \"Friend 1, Friend 2\" \"My Profile Name\"")
            };
            
            Plugin.ChatGui.Print(new SeString(payloads));
            return;
        }
        
        // Collect friend codes
        var targets = parsed[1].Split(",");
        
        // Extract the target profile name
        var profileName = parsed[2];

        // Get a list of all the profiles
        var profiles = await _customizePlusService.GetProfilesPlain().ConfigureAwait(false);
        
        // Check to see if the profile name is one of our profiles
        var profileId = Guid.Empty;
        foreach (var profile in profiles)
        {
            if (profile.Name == profileName)
            {
                profileId = profile.Guid;
                break;
            }
        }
        
        // If we didn't find an id, it doesn't exist
        if (profileId == Guid.Empty)
        {
            var payloads = new List<Payload>
            {
                new UIForegroundPayload(AetherRemoteColors.TextColorPurple),
                new TextPayload("[AetherRemote] "),
                UIForegroundPayload.UIForegroundOff,
                new TextPayload("Could not find profile. Type the profile exactly as you have it in customize, it is case-sensitive")
            };
            
            Plugin.ChatGui.Print(new SeString(payloads));
            return;
        }
        
        // Convert the profile
        if (await _customizePlusService.GetProfile(profileId).ConfigureAwait(false) is not { } raw)
        {
            var payloads = new List<Payload>
            {
                new UIForegroundPayload(AetherRemoteColors.TextColorPurple),
                new TextPayload("[AetherRemote] "),
                UIForegroundPayload.UIForegroundOff,
                new TextPayload("Could not find profile. Type \"/xllog\" and look for a warning or error to see what went wrong")
            };
            
            Plugin.ChatGui.Print(new SeString(payloads));
            return;
        }

        // Convert the profile into raw bytes
        var bytes = Encoding.UTF8.GetBytes(raw);
        
        // Grab the "should merge / additive" thing as well
        var shouldApplyAsAdditive = false;
        if (parsed.Length == 4)
            shouldApplyAsAdditive = bool.TryParse(parsed[3], out var value) && value;
     
        // Send
        await _networkCommandManager.SendCustomize(targets.ToList(), bytes, shouldApplyAsAdditive).ConfigureAwait(false);
    }
    
    /// <summary>
    ///     Handle an emote slash command
    /// </summary>
    private async Task HandleEmote(string args)
    {
        // Maybe this is better to do *before* passing it in?
        var parsed = ExtractArguments(args);
        if (parsed.Length < 3)
        {
            var payloads = new List<Payload>
            {
                new UIForegroundPayload(AetherRemoteColors.TextColorPurple),
                new TextPayload("[AetherRemote] "),
                UIForegroundPayload.UIForegroundOff,
                new TextPayload("Invalid syntax. Example /ar emote \"Friend 1, Friend 2\" dance")
            };
            
            Plugin.ChatGui.Print(new SeString(payloads));
            return;
        }

        // Collect friend codes
        var targets = parsed[1].Split(",");
        
        // Make sure the emote is valid
        var emote = parsed[2];
        if (_emoteService.Emotes.Contains(emote) is false)
        {
            var payloads = new List<Payload>
            {
                new UIForegroundPayload(AetherRemoteColors.TextColorPurple),
                new TextPayload("[AetherRemote] "),
                UIForegroundPayload.UIForegroundOff,
                new TextPayload("Invalid emote, make sure you type it as you would in chat (without the slash)")
            };
            
            Plugin.ChatGui.Print(new SeString(payloads));
            return;
        }
        
        // Grab the "include log" thing as well
        var displayLogMessage = false;
        if (parsed.Length == 4)
            displayLogMessage = bool.TryParse(parsed[3], out var value) && value;

        // Send
        await _networkCommandManager.SendEmote(targets.ToList(), emote, displayLogMessage).ConfigureAwait(false);
    }

    /// <summary>
    ///     Handle an emote slash command
    /// </summary>
    private async Task HandleSpeak(string args)
    {
        // Maybe this is better to do *before* passing it in?
        var parsed = ExtractArguments(args);
        if (parsed.Length < 3)
        {
            var payloads = new List<Payload>
            {
                new UIForegroundPayload(AetherRemoteColors.TextColorPurple),
                new TextPayload("[AetherRemote] "),
                UIForegroundPayload.UIForegroundOff,
                new TextPayload("Invalid syntax. Example /ar speak \"Friend 1, Friend 2\" cwl1 message")
            };
            
            Plugin.ChatGui.Print(new SeString(payloads));
            return;
        }
        
        // Collect friend codes
        var targets = parsed[1].Split(",").ToList();
        
        // Collect target channel
        var channelString = parsed[2];
        if (ToChatChannel(channelString) is not { } channel)
        {
            var payloads = new List<Payload>
            {
                new UIForegroundPayload(AetherRemoteColors.TextColorPurple),
                new TextPayload("[AetherRemote] "),
                UIForegroundPayload.UIForegroundOff,
                new TextPayload("Invalid chat channel")
            };
            
            Plugin.ChatGui.Print(new SeString(payloads));
            return;
        }
        
        // Some channels require additional work, so we don't know what these are yet
        string message;
        string? extra;

        // Process accordingly to the channel
        switch (channel)
        {
            case ChatChannel.Linkshell:
                message = parsed[3];
                extra = channelString.Length is 3
                    ? channelString[2].ToString()   // ls1
                    : channelString[9].ToString();  // linkshell1
                break;
            
            case ChatChannel.CrossWorldLinkshell:
                message = parsed[3];
                extra = channelString.Length is 4
                    ? channelString[3].ToString()   // cwl1
                    : channelString[11].ToString();  // cwlinkshell1
                break;
            
            case ChatChannel.Tell:
                message = parsed[4];
                extra = parsed[3]; // The tell target <Name>@<World>
                break;
            
            default:
                message = parsed[4];
                extra = null;
                break;
        }

        await _networkCommandManager.SendSpeak(targets, message, channel, extra);
    }

    /// <summary>
    ///     Handle an emote slash command
    /// </summary>
    private async Task HandleTransform(string args)
    {
        // Maybe this is better to do *before* passing it in?
        var parsed = ExtractArguments(args);
        if (parsed.Length < 3)
        {
            var payloads = new List<Payload>
            {
                new UIForegroundPayload(AetherRemoteColors.TextColorPurple),
                new TextPayload("[AetherRemote] "),
                UIForegroundPayload.UIForegroundOff,
                new TextPayload("Invalid syntax. Example /ar transform \"Friend 1, Friend 2\" \"My Design Name\"")
            };

            Plugin.ChatGui.Print(new SeString(payloads));
            return;
        }

        // Collect friend codes
        var targets = parsed[1].Split(",");

        // Extract the target profile name
        var designName = parsed[2];

        if (await _glamourerService.GetDesignList().ConfigureAwait(false) is not { } designs)
        {
            var payloads = new List<Payload>
            {
                new UIForegroundPayload(AetherRemoteColors.TextColorPurple),
                new TextPayload("[AetherRemote] "),
                UIForegroundPayload.UIForegroundOff,
                new TextPayload("Could not find designs. This should never happen, tell a developer!")
            };

            Plugin.ChatGui.Print(new SeString(payloads));
            return;
        }

        // Check to see if the profile name is one of our profiles
        var designId = Guid.Empty;
        foreach (var profile in designs)
        {
            if (profile.Name == designName)
            {
                designId = profile.Id;
                break;
            }
        }

        // If we didn't find an id, it doesn't exist
        if (designId == Guid.Empty)
        {
            var payloads = new List<Payload>
            {
                new UIForegroundPayload(AetherRemoteColors.TextColorPurple),
                new TextPayload("[AetherRemote] "),
                UIForegroundPayload.UIForegroundOff,
                new TextPayload("Could not find design. Type the profile exactly as you have it in glamourer, it is case-sensitive")
            };

            Plugin.ChatGui.Print(new SeString(payloads));
            return;
        }

        // Convert the profile
        if (await _glamourerService.GetDesignAsync(designId).ConfigureAwait(false) is not { } raw)
        {
            var payloads = new List<Payload>
            {
                new UIForegroundPayload(AetherRemoteColors.TextColorPurple),
                new TextPayload("[AetherRemote] "),
                UIForegroundPayload.UIForegroundOff,
                new TextPayload("Could not find design. Type \"/xllog\" and look for a warning or error to see what went wrong")
            };

            Plugin.ChatGui.Print(new SeString(payloads));
            return;
        }

        var applyType = parsed.Length < 4
            ? GlamourerApplyFlags.All
            : parsed[3] switch
            {
                "equipment" => GlamourerApplyFlags.Equipment,
                "customization" => GlamourerApplyFlags.Customization,
                _ => GlamourerApplyFlags.All
            };

        applyType |= GlamourerApplyFlags.Once;

        // Send
        await _networkCommandManager.SendTransformation(targets.ToList(), raw, applyType).ConfigureAwait(false);
    }

    private static ChatChannel? ToChatChannel(string channel)
    {
        if (channel.StartsWith("ls") || channel.StartsWith("linkshell"))
            return ChatChannel.Linkshell;

        if (channel.StartsWith("cwl") || channel.StartsWith("cwlinkshell"))
            return ChatChannel.CrossWorldLinkshell;
        
        return channel switch
        {
            "s" or "say" => ChatChannel.Say,
            "y" or "yell" => ChatChannel.Yell,
            "sh" or "shout" => ChatChannel.Shout,
            "t" or "tell" => ChatChannel.Tell,
            "p" or "party" => ChatChannel.Party,
            "a" or "alliance" => ChatChannel.Alliance,
            "fc" or "freecompany" => ChatChannel.FreeCompany,
            "pt" or "pvpteam" => ChatChannel.PvPTeam,
            "em" or "emote" => ChatChannel.Roleplay,
            "echo" => ChatChannel.Echo,
            _ => null
        };
    }
}