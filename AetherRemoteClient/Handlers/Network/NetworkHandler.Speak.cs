using System.Text;
using AetherRemoteCommon.Domain;
using AetherRemoteCommon.Domain.Enums;
using AetherRemoteCommon.Domain.Enums.Permissions;
using AetherRemoteCommon.Network.Domain;
using AetherRemoteCommon.Network.Domain.Payloads;
using AetherRemoteCommon.Network.Enums;
using AetherRemoteCommon.Util;

namespace AetherRemoteClient.Handlers.Network;

public partial class NetworkHandler
{
    private RoutedResponse<NoPayload> HandleSpeak(RoutedRequest<SpeakPayload> request)
    {
        Plugin.Log.Verbose($"{request}");
        
        var speakPermissions = request.Payload.ChatChannel.ToSpeakPermissions(request.Payload.Extra);
        var permissions = new ResolvedPermissions(PrimaryPermissions.None, speakPermissions, ElevatedPermissions.None);
        
        if (_friendsListService.Get(request.SenderFriendCode) is not { } sender)
            return new RoutedResponse<NoPayload>(RoutedResponseStatus.NotFriends);

        if (GetValidationError("Emote", sender, permissions) is { } error)
            return new RoutedResponse<NoPayload>(error);

        // Add the action to the action queue to be sent when available
        _actionQueueService.Enqueue(sender, request.Payload.Message, request.Payload.ChatChannel, request.Payload.Extra);

        // Build a proper log message with specific formatting
        var log = new StringBuilder();
        log.Append(sender.NoteOrFriendCode);
        log.Append(" made you say ");
        log.Append(request.Payload.Message);
        switch (request.Payload.ChatChannel)
        {
            case ChatChannel.Linkshell:
            case ChatChannel.CrossWorldLinkshell:
                log.Append(" in ");
                log.Append(request.Payload.ChatChannel.Beautify());
                log.Append(request.Payload.Extra);
                break;
            
            case ChatChannel.Tell:
                log.Append(" in a tell to ");
                log.Append(request.Payload.Extra);
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
                log.Append(request.Payload.ChatChannel.Beautify());
                log.Append(" chat");
                break;
        }
        
        // Add log to history
        _logService.Custom(log.ToString());
        return new RoutedResponse<NoPayload>(RoutedResponseStatus.Success);
    }
}