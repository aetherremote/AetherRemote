using System.Text;
using AetherRemoteClient.Services;
using AetherRemoteCommon.Domain.Enums;
using AetherRemoteCommon.Domain.Network;

namespace AetherRemoteClient.Handlers.Network;

/// <summary>
///     Handles a <see cref="EmoteAction"/>
/// </summary>
public class EmoteHandler(
    ChatService chatService,
    EmoteService emoteService,
    FriendsListService friendsListService,
    LogService logService,
    OverrideService overrideService)
{
    /// <summary>
    ///     <inheritdoc cref="EmoteHandler"/>
    /// </summary>
    public void Handle(EmoteAction action)
    {
        // Not friends
        if (friendsListService.Get(action.SenderFriendCode) is not { } friend)
        {
            logService.NotFriends("Emote", action.SenderFriendCode);
            return;
        }

        // Plugin in safe mode
        if (Plugin.Configuration.SafeMode)
        {
            logService.SafeMode("Emote", friend.NoteOrFriendCode);
            return;
        }

        // Overriding emotes
        if (overrideService.HasActiveOverride(PrimaryPermissions.Emote))
        {
            logService.Override("Emote", friend.NoteOrFriendCode);
            return;
        }

        // Lacking permissions for emoting
        if (friend.PermissionsGrantedToFriend.Primary.HasFlag(PrimaryPermissions.Emote) is false)
        {
            logService.LackingPermissions("Emote", friend.NoteOrFriendCode);
            return;
        }

        // Check if real emote
        if (emoteService.Emotes.Contains(action.Emote) is false)
        {
            logService.InvalidData("Emote", friend.NoteOrFriendCode);
            return;
        }

        // Construct command
        var command = new StringBuilder();
        command.Append('/');
        command.Append(action.Emote);
        if (action.DisplayLogMessage is false)
            command.Append(" <mo>");
        
        // Execute command
        chatService.SendMessage(command.ToString());
        
        // Log success
        logService.Custom($"{friend.NoteOrFriendCode} made you do the {action.Emote} emote");
    }
}