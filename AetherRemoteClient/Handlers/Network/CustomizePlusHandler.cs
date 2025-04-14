using System;
using System.Threading.Tasks;
using AetherRemoteClient.Ipc;
using AetherRemoteClient.Services;
using AetherRemoteCommon.Domain.Enums;
using AetherRemoteCommon.Domain.Network;
using AetherRemoteCommon.Util;

namespace AetherRemoteClient.Handlers.Network;

public class CustomizePlusHandler(
    FriendsListService friendsListService,
    OverrideService overrideService,
    LogService logService,
    CustomizePlusIpc customize)
{
    /// <summary>
    ///     <inheritdoc cref="MoodlesHandler"/>
    /// </summary>
    public async Task Handle(CustomizePlusAction action)
    {
        Plugin.Log.Info($"{action}");
        
        // Not friends
        if (friendsListService.Get(action.SenderFriendCode) is not { } friend)
        {
            logService.NotFriends("CustomizePlus", action.SenderFriendCode);
            return;
        }

        // Plugin in safe mode
        if (Plugin.Configuration.SafeMode)
        {
            logService.SafeMode("CustomizePlus", friend.NoteOrFriendCode);
            return;
        }

        // Overriding moodles
        if (overrideService.HasActiveOverride(PrimaryPermissions.CustomizePlus))
        {
            logService.Override("CustomizePlus", friend.NoteOrFriendCode);
            return;
        }

        // Lacking permissions for moodles
        if (friend.PermissionsGrantedToFriend.Has(PrimaryPermissions.CustomizePlus) is false)
        {
            logService.LackingPermissions("CustomizePlus", friend.NoteOrFriendCode);
            return;
        }

        try
        {
            if (await customize.ApplyCustomize(action.Customize) is false)
            {
                Plugin.Log.Warning("[CustomizePlusHandler] Unable to apply customize");
                return;
            }
            
            logService.Custom($"{friend.NoteOrFriendCode} applied a customize plus template to you");
        }
        catch (Exception e)
        {
            logService.Custom($"{friend.NoteOrFriendCode} tried to apply a customization template to you but failed unexpectedly");
            Plugin.Log.Error($"Unexpected exception while handling customize plus action, {e.Message}");
        }
    }
}