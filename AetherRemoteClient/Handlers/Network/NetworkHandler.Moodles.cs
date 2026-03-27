using System.Threading.Tasks;
using AetherRemoteClient.Services;
using AetherRemoteCommon.Domain;
using AetherRemoteCommon.Domain.Enums;
using AetherRemoteCommon.Domain.Enums.Permissions;
using AetherRemoteCommon.Domain.Network;
using AetherRemoteCommon.Domain.Network.Moodles;

namespace AetherRemoteClient.Handlers.Network;

public partial class NetworkHandler
{
    private static readonly ResolvedPermissions MoodlesPermissions = new(PrimaryPermissions.Moodles, SpeakPermissions.None, ElevatedPermissions.None);
    
    private async Task<ActionResult<Unit>> HandleMoodles(MoodlesCommand request)
    {
        Plugin.Log.Verbose($"{request}");

        // If the client has not accepted the agreement
        if (AgreementsService.HasAgreedTo(AgreementsService.Agreements.MoodlesWarning) is false)
            return ActionResultBuilder.Fail(ActionResultEc.HasNotAcceptedAgreement);
        
        var sender = TryGetFriendWithCorrectPermissions("Moodles", request.SenderFriendCode, MoodlesPermissions);
        if (sender.Result is not ActionResultEc.Success)
            return ActionResultBuilder.Fail(sender.Result);
        
        if (sender.Value is not { } friend)
            return ActionResultBuilder.Fail(ActionResultEc.ValueNotSet);
        
        // Attempt to apply the Moodle
        if (await _moodlesService.ApplyMoodle(request.Info).ConfigureAwait(false))
        {
            _logService.Custom($"{friend.NoteOrFriendCode} applied {MoodlesService.RemoveTagsFromTitle(request.Info.Title)} to you");
            return ActionResultBuilder.Ok();
        }

        _logService.Custom($"{friend.NoteOrFriendCode} tried to apply a Moodle to you but an error occurred");
        return ActionResultBuilder.Fail(ActionResultEc.Unknown);
    }
}