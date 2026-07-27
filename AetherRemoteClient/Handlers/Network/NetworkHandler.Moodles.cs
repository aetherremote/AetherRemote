using System.Threading.Tasks;
using AetherRemoteClient.Services.Dependencies;
using AetherRemoteCommon.Domain;
using AetherRemoteCommon.Domain.Enums.Permissions;
using AetherRemoteCommon.Network.Domain;
using AetherRemoteCommon.Network.Domain.Payloads;
using AetherRemoteCommon.Network.Enums;

namespace AetherRemoteClient.Handlers.Network;

public partial class NetworkHandler
{
    private static readonly ResolvedPermissions MoodlesPermissions = new(PrimaryPermissions.Moodles, SpeakPermissions.None, ElevatedPermissions.None);
    
    private async Task<RoutedResponse<NoPayload>> HandleMoodles(RoutedRequest<MoodlesPayload> request)
    {
        Plugin.Log.Verbose($"{request}");
        
        if (_friendsListService.Get(request.SenderFriendCode) is not { } sender)
            return new RoutedResponse<NoPayload>(RoutedResponseStatus.NotFriends);

        if (GetValidationError("Moodles", sender, MoodlesPermissions) is { } error)
            return new RoutedResponse<NoPayload>(error);
        
        if (await _moodlesService.ApplyMoodle(request.Payload.Info).ConfigureAwait(false))
        {
            _logService.Custom($"{sender.NoteOrFriendCode} applied {MoodlesService.RemoveTagsFromTitle(request.Payload.Info.Title)} to you");
            return new RoutedResponse<NoPayload>(RoutedResponseStatus.Success);
        }

        _logService.Custom($"{sender.NoteOrFriendCode} tried to apply a Moodle to you but an error occurred");
        return new RoutedResponse<NoPayload>(RoutedResponseStatus.Unknown);
    }
}