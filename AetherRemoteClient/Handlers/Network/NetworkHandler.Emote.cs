using System.Text;
using AetherRemoteClient.Services;
using AetherRemoteCommon.Domain;
using AetherRemoteCommon.Domain.Enums.Permissions;
using AetherRemoteCommon.Network.Domain;
using AetherRemoteCommon.Network.Domain.Payloads;
using AetherRemoteCommon.Network.Enums;

namespace AetherRemoteClient.Handlers.Network;

public partial class NetworkHandler
{
    private static readonly ResolvedPermissions EmotePermissions = new(PrimaryPermissions.Emote, SpeakPermissions.None, ElevatedPermissions.None);

    private RoutedResponse<NoPayload> HandleEmoteCommand(RoutedRequest<EmotePayload> request)
    {
        Plugin.Log.Verbose($"{request}");

        if (_friendsListService.Get(request.SenderFriendCode) is not { } sender)
            return new RoutedResponse<NoPayload>(RoutedResponseStatus.NotFriends);

        if (GetValidationError("Emote", sender, EmotePermissions) is { } error)
            return new RoutedResponse<NoPayload>(error);
        
        // Check if real emote
        if (_emoteService.Emotes.Contains(request.Payload.Emote) is false)
        {
            _logService.InvalidData("Emote", sender.NoteOrFriendCode);
            return new RoutedResponse<NoPayload>(RoutedResponseStatus.BadRequest);
        }

        // Construct command
        var command = new StringBuilder();
        command.Append('/');
        command.Append(request.Payload.Emote);
        if (request.Payload.DisplayLogMessage is false)
            command.Append(" <mo>");
        
        // Execute command
        ChatService.SendMessage(command.ToString());
        
        // Log success
        _logService.Custom($"{sender.NoteOrFriendCode} made you do the {request.Payload.Emote} emote");
        
        // Success
        return new RoutedResponse<NoPayload>(RoutedResponseStatus.Success);
    }
}