using System.Text;
using AetherRemoteClient.Managers;
using AetherRemoteClient.Services;
using AetherRemoteCommon.Domain;
using AetherRemoteCommon.Domain.Enums;
using AetherRemoteCommon.Domain.Enums.Permissions;
using AetherRemoteCommon.Domain.Network;
using AetherRemoteCommon.Domain.Network.Emote;

namespace AetherRemoteClient.Handlers.Network;

// ReSharper disable once ConvertToPrimaryConstructor

/// <summary>
///     Handles a <see cref="EmoteForwardedRequest"/>
/// </summary>
public class EmoteHandler(EmoteService emoteService, LogService logService, PermissionsCheckerManager permissionsCheckerManager)
{
    // Const
    private const string Operation = "Emote";
    private static readonly UserPermissions Permissions = new(PrimaryPermissions2.Emote, SpeakPermissions2.None, ElevatedPermissions.None);
    
    /// <summary>
    ///     <inheritdoc cref="EmoteHandler"/>
    /// </summary>
    public ActionResult<Unit> Handle(EmoteForwardedRequest request)
    {
        Plugin.Log.Info($"{request}");
        
        var placeholder = permissionsCheckerManager.GetSenderAndCheckPermissions(Operation, request.SenderFriendCode, Permissions);
        if (placeholder.Result is not ActionResultEc.Success)
            return ActionResultBuilder.Fail(placeholder.Result);
        
        if (placeholder.Value is not { } friend)
            return ActionResultBuilder.Fail(ActionResultEc.ValueNotSet);

        // Check if real emote
        if (emoteService.Emotes.Contains(request.Emote) is false)
        {
            logService.InvalidData(Operation, friend.NoteOrFriendCode);
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