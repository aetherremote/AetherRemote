using System.Text;
using AetherRemoteClient.Managers;
using AetherRemoteClient.Services;
using AetherRemoteCommon.Domain;
using AetherRemoteCommon.Domain.Enums;
using AetherRemoteCommon.Domain.Enums.Permissions;
using AetherRemoteCommon.Domain.Network;
using AetherRemoteCommon.Domain.Network.Speak;
using AetherRemoteCommon.Util;

namespace AetherRemoteClient.Handlers.Network;

// ReSharper disable once ConvertToPrimaryConstructor

/// <summary>
///     Handles a <see cref="SpeakForwardedRequest"/>
/// </summary>
public class SpeakHandler(ActionQueueService actionQueueService, LogService logService, PermissionsCheckerManager permissionsCheckerManager)
{
    // Const
    private const string Operation = "Speak";
    
    /// <summary>
    ///     <inheritdoc cref="SpeakHandler"/>
    /// </summary>
    public ActionResult<Unit> Handle(SpeakForwardedRequest request)
    {
        Plugin.Log.Info($"{request}");

        var speak = request.ChatChannel.ToSpeakPermissions(request.Extra);
        var permissions = new UserPermissions(PrimaryPermissions2.None, speak, ElevatedPermissions.None);
        var placeholder = permissionsCheckerManager.GetSenderAndCheckPermissions(Operation, request.SenderFriendCode, permissions);
        if (placeholder.Result is not ActionResultEc.Success)
            return ActionResultBuilder.Fail(placeholder.Result);
        
        if (placeholder.Value is not { } friend)
            return ActionResultBuilder.Fail(ActionResultEc.ValueNotSet);

        // Add the action to the action queue to be sent when available
        actionQueueService.Enqueue(friend, request.Message, request.ChatChannel, request.Extra);

        // Build a proper log message with specific formatting
        var log = new StringBuilder();
        log.Append(friend.NoteOrFriendCode);
        log.Append(" made you say ");
        log.Append(request.Message);
        switch (request.ChatChannel)
        {
            case ChatChannel.Linkshell:
            case ChatChannel.CrossWorldLinkshell:
                log.Append(" in ");
                log.Append(request.ChatChannel.Beautify());
                log.Append(request.Extra);
                break;
            
            case ChatChannel.Tell:
                log.Append(" in a tell to ");
                log.Append(request.Extra);
                break;

            case ChatChannel.Say:
            case ChatChannel.Roleplay:
            case ChatChannel.Echo:
            case ChatChannel.Yell:
            case ChatChannel.Shout:
            case ChatChannel.Party:
            case ChatChannel.Alliance:
            case ChatChannel.FreeCompany:
            case ChatChannel.PvPTeam:
            default:
                log.Append(" in ");
                log.Append(request.ChatChannel.Beautify());
                log.Append(" chat");
                break;
        }
        
        // Add log to history
        logService.Custom(log.ToString());
        return ActionResultBuilder.Ok();
    }
}