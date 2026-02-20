using System.Text;
using AetherRemoteCommon.Domain;
using AetherRemoteCommon.Domain.Enums;
using AetherRemoteCommon.Domain.Enums.Permissions;
using AetherRemoteCommon.Domain.Network;
using AetherRemoteCommon.Domain.Network.Speak;
using AetherRemoteCommon.Util;

namespace AetherRemoteClient.Handlers.Network;

public partial class NetworkHandler
{
    private ActionResult<Unit> HandleSpeak(SpeakCommand request)
    {
        Plugin.Log.Verbose($"{request}");
        
        var speakPermissions = request.ChatChannel.ToSpeakPermissions(request.Extra);
        var permissions = new ResolvedPermissions(PrimaryPermissions.None, speakPermissions, ElevatedPermissions.None);
        
        var sender = TryGetFriendWithCorrectPermissions("Speak", request.SenderFriendCode, permissions);
        if (sender.Result is not ActionResultEc.Success)
            return ActionResultBuilder.Fail(sender.Result);
        
        if (sender.Value is not { } friend)
            return ActionResultBuilder.Fail(ActionResultEc.ValueNotSet);

        // Add the action to the action queue to be sent when available
        _actionQueueService.Enqueue(friend, request.Message, request.ChatChannel, request.Extra);

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
        _logService.Custom(log.ToString());
        return ActionResultBuilder.Ok();
    }
}