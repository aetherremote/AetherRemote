using System;
using System.Threading.Tasks;
using AetherRemoteClient.Utils.Extensions;
using AetherRemoteCommon.Domain;
using AetherRemoteCommon.Domain.Enums;
using AetherRemoteCommon.Domain.Enums.Permissions;
using AetherRemoteCommon.Domain.Network;
using AetherRemoteCommon.Domain.Network.Honorific;

namespace AetherRemoteClient.Handlers.Network;

public partial class NetworkHandler
{
    private static readonly ResolvedPermissions HonorificPermissions = new(PrimaryPermissions.Honorific, SpeakPermissions.None, ElevatedPermissions.None);
    
    private async Task<ActionResult<Unit>> HandleHonorific(HonorificCommand request)
    {
        Plugin.Log.Verbose($"{request}");
        
        var sender = TryGetFriendWithCorrectPermissions("Honorific", request.SenderFriendCode, HonorificPermissions);
        if (sender.Result is not ActionResultEc.Success)
            return ActionResultBuilder.Fail(sender.Result);
        
        if (sender.Value is not { } friend)
            return ActionResultBuilder.Fail(ActionResultEc.ValueNotSet);
        
        try
        {
            if (await _honorificService.SetCharacterTitle(request.Honorific.ToHonorificDto()).ConfigureAwait(false))
            {
                _statusManager.SetHonorific(friend);
                _logService.Custom($"{friend.NoteOrFriendCode} applied the {request.Honorific.Title} honorific to you");
                return ActionResultBuilder.Ok();
            }
            
            _logService.Custom($"{friend.NoteOrFriendCode} failed to apply an honorific to you");
            return ActionResultBuilder.Fail(ActionResultEc.ClientPluginDependency);
        }
        catch (Exception e)
        {
            _logService.Custom($"{friend.NoteOrFriendCode} unexpectedly failed to apply an honorific to you");
            Plugin.Log.Warning($"[HonorificHandler.Handle] {e}");
            return ActionResultBuilder.Fail(ActionResultEc.Unknown);
        }
    }
}