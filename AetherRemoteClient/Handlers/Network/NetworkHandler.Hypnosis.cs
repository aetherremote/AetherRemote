using System.Threading.Tasks;
using AetherRemoteCommon.Domain;
using AetherRemoteCommon.Domain.Enums;
using AetherRemoteCommon.Domain.Enums.Permissions;
using AetherRemoteCommon.Domain.Network;
using AetherRemoteCommon.Domain.Network.Hypnosis;

namespace AetherRemoteClient.Handlers.Network;

public partial class NetworkHandler
{
    private static readonly ResolvedPermissions HypnosisPermissions = new(PrimaryPermissions.Hypnosis, SpeakPermissions.None, ElevatedPermissions.None);
    
    private async Task<ActionResult<Unit>> HandleHypnosis(HypnosisCommand request)
    {
        Plugin.Log.Verbose($"{request}");

        var sender = TryGetFriendWithCorrectPermissions("Hypnosis", request.SenderFriendCode, HypnosisPermissions);
        if (sender.Result is not ActionResultEc.Success)
            return ActionResultBuilder.Fail(sender.Result);
        
        if (sender.Value is not { } friend)
            return ActionResultBuilder.Fail(ActionResultEc.ValueNotSet);
        
        // If you're already being hypnotized
        if (_hypnosisManager.IsBeingHypnotized)
        {
            // If the sender is the one who initiated it
            if (_hypnosisManager.Hypnotist?.FriendCode == request.SenderFriendCode)
            {
                // Do nothing
            }
            else
            {
                // Bounce their request
                _logService.Custom($"Rejected hypnosis spiral from {friend.NoteOrFriendCode} because you're already being hypnotized");
                return ActionResultBuilder.Fail(ActionResultEc.ClientBeingHypnotized);
            }
        }
        
        // Begin the hypnosis
        await _hypnosisManager.Hypnotize(friend, request.Data);
        
        // Log
        _statusManager.SetHypnosis(friend);
        _logService.Custom($"{friend.NoteOrFriendCode} began to hypnotize you");
        return ActionResultBuilder.Ok();
    }
}