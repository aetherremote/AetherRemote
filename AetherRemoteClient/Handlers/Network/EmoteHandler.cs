using System.Text;
using AetherRemoteClient.Services;
using AetherRemoteCommon.Domain.Enums;
using AetherRemoteCommon.V2.Domain;
using AetherRemoteCommon.V2.Domain.Enum;
using AetherRemoteCommon.V2.Domain.Network;
using AetherRemoteCommon.V2.Domain.Network.Emote;

namespace AetherRemoteClient.Handlers.Network;

/// <summary>
///     Handles a <see cref="EmoteForwardedRequest"/>
/// </summary>
public class EmoteHandler(
    EmoteService emoteService,
    FriendsListService friendsListService,
    LogService logService,
    OverrideService overrideService)
{
    /// <summary>
    ///     <inheritdoc cref="EmoteHandler"/>
    /// </summary>
    public ActionResult<Unit> Handle(EmoteForwardedRequest request)
    {
        Plugin.Log.Info($"{request}");
        
        // Not friends
        if (friendsListService.Get(request.SenderFriendCode) is not { } friend)
        {
            logService.NotFriends("Emote", request.SenderFriendCode);
            return ActionResultBuilder.Fail(ActionResultEc.ClientNotFriends);
        }

        // Plugin in safe mode
        if (Plugin.Configuration.SafeMode)
        {
            logService.SafeMode("Emote", friend.NoteOrFriendCode);
            return ActionResultBuilder.Fail(ActionResultEc.ClientInSafeMode);
        }

        // Overriding emotes
        if (overrideService.HasActiveOverride(PrimaryPermissions.Emote))
        {
            logService.Override("Emote", friend.NoteOrFriendCode);
            return ActionResultBuilder.Fail(ActionResultEc.ClientHasOverride);
        }

        // Lacking permissions for emoting
        if (friend.PermissionsGrantedToFriend.Primary.HasFlag(PrimaryPermissions.Emote) is false)
        {
            logService.LackingPermissions("Emote", friend.NoteOrFriendCode);
            return ActionResultBuilder.Fail(ActionResultEc.ClientHasNotGrantedSenderPermissions);
        }

        // Check if real emote
        if (emoteService.Emotes.Contains(request.Emote) is false)
        {
            logService.InvalidData("Emote", friend.NoteOrFriendCode);
            return ActionResultBuilder.Fail(ActionResultEc.ClientBadData);
        }

        // Construct command
        var command = new StringBuilder();
        command.Append('/');
        command.Append(request.Emote);
        if (request.DisplayLogMessage is false)
            command.Append(" <mo>");
        
        // Execute command
        ChatService.SendMessage(command.ToString());
        
        // Log success
        logService.Custom($"{friend.NoteOrFriendCode} made you do the {request.Emote} emote");
        
        // Success
        return ActionResultBuilder.Ok();
    }
}