using System.Text;
using AetherRemoteClient.Services;
using AetherRemoteCommon.Domain;
using AetherRemoteCommon.Domain.Enums;
using AetherRemoteCommon.Domain.Enums.Permissions;
using AetherRemoteCommon.Domain.Network;
using AetherRemoteCommon.Domain.Network.Emote;

namespace AetherRemoteClient.Handlers.Network;

public partial class NetworkHandler
{
    private static readonly ResolvedPermissions EmotePermissions = new(PrimaryPermissions.Emote, SpeakPermissions.None, ElevatedPermissions.None);

    private ActionResult<Unit> HandleEmoteCommand(EmoteCommand request)
    {
        Plugin.Log.Verbose($"{request}");
        
        var sender = TryGetFriendWithCorrectPermissions("Emote", request.SenderFriendCode, EmotePermissions);
        if (sender.Result is not ActionResultEc.Success)
            return ActionResultBuilder.Fail(sender.Result);
        
        if (sender.Value is not { } friend)
            return ActionResultBuilder.Fail(ActionResultEc.ValueNotSet);

        // Check if real emote
        if (_emoteService.Emotes.Contains(request.Emote) is false)
        {
            _logService.InvalidData("Emote", friend.NoteOrFriendCode);
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
        _logService.Custom($"{friend.NoteOrFriendCode} made you do the {request.Emote} emote");
        
        // Success
        return ActionResultBuilder.Ok();
    }
}