using System.Text;
using AetherRemoteClient.Managers;
using AetherRemoteClient.Services;
using AetherRemoteCommon.Domain;
using AetherRemoteCommon.Domain.Enums;
using AetherRemoteCommon.Domain.Enums.Permissions;
using AetherRemoteCommon.Domain.Network;
using AetherRemoteCommon.V2.Domain;
using AetherRemoteCommon.V2.Domain.Network.Emote;

namespace AetherRemoteClient.Handlers.Network;

/// <summary>
///     Handles a <see cref="EmoteForwardedRequest"/>
/// </summary>
public class EmoteHandler(
    EmoteService emoteService,
    LogService logService,
    ForwardedRequestManager forwardedRequestManager)
{
    // Const
    private const string Operation = "Emote";
    private const PrimaryPermissions2 Permissions = PrimaryPermissions2.Emote;
    
    /// <summary>
    ///     <inheritdoc cref="EmoteHandler"/>
    /// </summary>
    public ActionResult<Unit> Handle(EmoteForwardedRequest request)
    {
        Plugin.Log.Info($"{request}");
        
        var placeholder = forwardedRequestManager.Placehold(Operation, request.SenderFriendCode, Permissions);
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