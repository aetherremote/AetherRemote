using System.Text;
using AetherRemoteClient.Managers;
using AetherRemoteClient.Services;
using AetherRemoteCommon.Domain.Enums;
using AetherRemoteCommon.Domain.Network;
using AetherRemoteCommon.Util;
using AetherRemoteCommon.V2.Domain.Network.Speak;
using Microsoft.Extensions.Primitives;

namespace AetherRemoteClient.Handlers.Network;

/// <summary>
///     Handles a <see cref="SpeakForwardedRequest"/>
/// </summary>
public class SpeakHandler(
    FriendsListService friendsListService,
    LogService logService,
    OverrideService overrideService,
    ActionQueueService actionQueueService)
{
    /// <summary>
    ///     <inheritdoc cref="SpeakHandler"/>
    /// </summary>
    public void Handle(SpeakForwardedRequest forwardedRequest)
    {
        // Not friends
        if (friendsListService.Get(forwardedRequest.SenderFriendCode) is not { } friend)
        {
            logService.NotFriends("Speak", forwardedRequest.SenderFriendCode);
            return;
        }

        // Plugin in safe mode
        if (Plugin.Configuration.SafeMode)
        {
            logService.SafeMode("Speak", friend.NoteOrFriendCode);
            return;
        }

        // Overriding speaking
        if (overrideService.HasActiveOverride(PrimaryPermissions.Speak))
        {
            logService.Override("Speak", friend.NoteOrFriendCode);
            return;
        }

        // Lacking permissions for speaking
        if (friend.PermissionsGrantedToFriend.Has(PrimaryPermissions.Speak) is false)
        {
            logService.LackingPermissions("Speak", friend.NoteOrFriendCode);
            return;
        }

        // Handle special cases for parsing linkshell number in action
        if (forwardedRequest.ChatChannel is ChatChannel.Linkshell or ChatChannel.CrossWorldLinkshell)
        {
            // Get linkshell number
            if (int.TryParse(forwardedRequest.Extra, out var linkshellNumber) is false)
            {
                logService.InvalidData("Speak", friend.NoteOrFriendCode);
                return;
            }

            // Convert to proper enum linkshell permissions
            var linkshellPermissions = forwardedRequest.ChatChannel.ToLinkshellPermissions(linkshellNumber);
            if (linkshellPermissions is LinkshellPermissions.None)
            {
                logService.InvalidData("Speak", friend.NoteOrFriendCode);
                return;
            }

            // Overriding provided linkshell permissions
            if (overrideService.HasActiveOverride(linkshellPermissions))
            {
                logService.Override("Speak", friend.NoteOrFriendCode);
                return;
            }

            // Lacking permissions for provided linkshell permissions
            if (friend.PermissionsGrantedToFriend.Has(linkshellPermissions) is false)
            {
                logService.LackingPermissions("Speak", friend.NoteOrFriendCode);
                return;
            }
        }
        else
        {
            // Convert to proper enum primary permissions
            var primaryPermissions = forwardedRequest.ChatChannel.ToPrimaryPermissions();
            if (primaryPermissions is PrimaryPermissions.None)
            {
                logService.InvalidData("Speak", friend.NoteOrFriendCode);
                return;
            }

            // Overriding provided primary permissions
            if (overrideService.HasActiveOverride(primaryPermissions))
            {
                logService.Override("Speak", friend.NoteOrFriendCode);
                return;
            }

            // Lacking permissions for provided primary permissions
            if (friend.PermissionsGrantedToFriend.Has(primaryPermissions) is false)
            {
                logService.LackingPermissions("Speak", friend.NoteOrFriendCode);
                return;
            }
        }

        // Add the action to the action queue to be sent when available
        actionQueueService.Enqueue(friend, forwardedRequest.Message, forwardedRequest.ChatChannel, forwardedRequest.Extra);

        // Build a proper log message with specific formatting
        var log = new StringBuilder();
        log.Append(friend.NoteOrFriendCode);
        log.Append(" made you say ");
        log.Append(forwardedRequest.Message);
        switch (forwardedRequest.ChatChannel)
        {
            case ChatChannel.Linkshell:
            case ChatChannel.CrossWorldLinkshell:
                log.Append(" in ");
                log.Append(forwardedRequest.ChatChannel.Beautify());
                log.Append(forwardedRequest.Extra);
                break;
            
            case ChatChannel.Tell:
                log.Append(" in a tell to ");
                log.Append(forwardedRequest.Extra);
                break;

            case ChatChannel.Say:
            case ChatChannel.Roleplay:
            case ChatChannel.Echo:
            case ChatChannel.Yell:
            case ChatChannel.Shout:
            case ChatChannel.Party:
            case ChatChannel.Alliance:
            case ChatChannel.FreeCompany:
            case ChatChannel.NoviceNetwork:
            case ChatChannel.PvPTeam:
            default:
                log.Append(" in ");
                log.Append(forwardedRequest.ChatChannel.Beautify());
                log.Append(" chat");
                break;
        }
        
        // Add log to history
        logService.Custom(log.ToString());
    }
}