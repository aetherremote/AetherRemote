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
    private async Task HandleSpeak(string args)
    {
        // /ar speak | friendCode1, friendCode2 | cwl1 | message
        // /ar speak | friendCode1, friendCode2 | tell My Name@My Homeworld | message
        var arguments = args.Split('|', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        // If the number of arguments is less than the amount required
        if (arguments.Length < 4)
        {
            SendChatMessage("Invalid number of arguments");
            return;
        }
        
        // Extract arguments
        var argsTargets = arguments[1];
        var argsChannel = arguments[2];
        var argsMessage = arguments[3];
        
        // Format Targets
        var targets = argsTargets.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        
        // Format the channel, which is the bulk of the complexity
        ChatChannel channel;
        string? extra;

        // Tells need extra attention because they look like "tell My Name@My Homeworld"
        if (argsChannel.StartsWith('t') || argsChannel.StartsWith("tell"))
        {
            channel = ChatChannel.Tell;
            try
            {
                // I'm lazy I'm sorry
                extra = argsChannel[5..].TrimEnd();
            }
            catch (Exception)
            {
                SendChatMessage("Unable to parse channel");
                return;
            }
        }
        else if (argsChannel.StartsWith("ls") || argsChannel.StartsWith("linkshell"))
        {
            channel = ChatChannel.Linkshell;
            try
            {
                // I'm lazy I'm sorry
                extra = argsChannel.StartsWith("ls") ? argsChannel[2].ToString() : argsChannel[9].ToString();
            }
            catch (Exception)
            {
                SendChatMessage("Unable to parse channel");
                return;
            }
        }
        else if (argsChannel.StartsWith("cwl") || argsChannel.StartsWith("cwlinkshell"))
        {
            channel = ChatChannel.CrossWorldLinkshell;
            try
            {
                // I'm lazy I'm sorry
                extra = argsChannel.StartsWith("ls") ? argsChannel[3].ToString() : argsChannel[11].ToString();
            }
            catch (Exception)
            {
                SendChatMessage("Unable to parse channel");
                return;
            }
        }
        else
        {
            ChatChannel? parsedChannel = argsChannel switch
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

            if (parsedChannel is null)
            {
                SendChatMessage("Unable to parse channel");
                return;
            }
            
            channel = parsedChannel.Value;
            extra = null;
        }
        
        await _networkCommandManager.SendSpeak(targets.ToList(), argsMessage, channel, extra);
    }
}