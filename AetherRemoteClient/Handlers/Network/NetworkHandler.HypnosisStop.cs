using System;
using System.Threading.Tasks;
using AetherRemoteCommon.Domain;
using AetherRemoteCommon.Domain.Enums;
using AetherRemoteCommon.Domain.Enums.Permissions;
using AetherRemoteCommon.Domain.Network;
using AetherRemoteCommon.Domain.Network.HypnosisStop;

namespace AetherRemoteClient.Handlers.Network;

public partial class NetworkHandler
{
    private static readonly ResolvedPermissions HypnosisStopPermissions = new(PrimaryPermissions.Hypnosis, SpeakPermissions.None, ElevatedPermissions.None);
    
    private async Task<ActionResult<Unit>> HandleHypnosisStop(HypnosisStopCommand request)
    {
        Plugin.Log.Verbose($"{request}");
        
        var sender = TryGetFriendWithCorrectPermissions("HypnosisStop", request.SenderFriendCode, HypnosisStopPermissions);
        if (sender.Result is not ActionResultEc.Success)
            return ActionResultBuilder.Fail(sender.Result);
        
        if (sender.Value is not { } friend)
            return ActionResultBuilder.Fail(ActionResultEc.ValueNotSet);

        // If they're not being hypnotized, No-Op
        if (_hypnosisManager.IsBeingHypnotized is false)
            return ActionResultBuilder.Ok();
        
        // If they're the one who sent the hypnosis request in the first place
        if (_hypnosisManager.Hypnotist?.FriendCode == request.SenderFriendCode)
        {
            await Plugin.RunOnFramework((Action)(() => _hypnosisManager.Wake())).ConfigureAwait(false);
            _statusManager.ClearHypnosis();
            return ActionResultBuilder.Ok();
        }

        // Bounce their request
        _logService.Custom($"Rejected hypnosis spiral from {friend.NoteOrFriendCode} because you're already being hypnotized");
        return ActionResultBuilder.Fail(ActionResultEc.ClientBeingHypnotized);
    }
}