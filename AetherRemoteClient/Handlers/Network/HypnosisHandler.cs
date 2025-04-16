using System.Threading.Tasks;
using AetherRemoteClient.Services;
using AetherRemoteCommon.Domain;
using AetherRemoteCommon.Domain.Enums;
using AetherRemoteCommon.Domain.Network;
using AetherRemoteCommon.Util;

namespace AetherRemoteClient.Handlers.Network;

public class HypnosisHandler(
    FriendsListService friendsListService,
    OverrideService overrideService,
    LogService logService,
    SpiralService spiralService)
{
    /// <summary>
    ///     <inheritdoc cref="MoodlesHandler"/>
    /// </summary>
    public void Handle(HypnosisAction action)
    {
        Plugin.Log.Info($"{action}");

        // Not friends
        if (friendsListService.Get(action.SenderFriendCode) is not { } friend)
        {
            logService.NotFriends("Hypnosis", action.SenderFriendCode);
            return;
        }

        // Plugin in safe mode
        if (Plugin.Configuration.SafeMode)
        {
            logService.SafeMode("Hypnosis", friend.NoteOrFriendCode);
            return;
        }

        // Overriding hypnosis
        if (overrideService.HasActiveOverride(PrimaryPermissions.Hypnosis))
        {
            logService.Override("Hypnosis", friend.NoteOrFriendCode);
            return;
        }

        // Lacking permissions for hypnosis
        if (friend.PermissionsGrantedToFriend.Has(PrimaryPermissions.Hypnosis) is false)
        {
            logService.LackingPermissions("Hypnosis", friend.NoteOrFriendCode);
            return;
        }
        
        // Already being hypnotized
        if (spiralService.IsBeingHypnotized)
        {
            logService.Custom($"Rejected hypnosis spiral from {friend.NoteOrFriendCode} because you're already being hypnotized");
            return;
        }
        
        spiralService.StartSpiral(action.Spiral, friend.NoteOrFriendCode);
        logService.Custom($"{friend.NoteOrFriendCode} began to hypnotize you");
    }
}