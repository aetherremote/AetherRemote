using System;
using System.Collections.Generic;
using System.Linq;
using AetherRemoteClient.Services;
using AetherRemoteClient.Utils;
using AetherRemoteCommon.Domain;
using AetherRemoteCommon.Domain.Enums;
using AetherRemoteCommon.Domain.Network;
using AetherRemoteCommon.Util;

namespace AetherRemoteClient.UI.Views.Twinning;

public class TwinningViewUiController(
    FriendsListService friendsListService,
    IdentityService identityService,
    NetworkService networkService)
{
    public bool SwapMods;

    public async void Twin()
    {
        try
        {
            if (Plugin.ClientState.LocalPlayer is not { } player)
                return;

            var input = new TwinningRequest
            {
                TargetFriendCodes = friendsListService.Selected.Select(friend => friend.FriendCode).ToList(),
                Identity = new CharacterIdentity
                {
                    GameObjectName = player.Name.ToString(),
                    CharacterName = identityService.Identity
                },
                SwapMods = SwapMods
            };

            var response = await networkService.InvokeAsync<TwinningRequest, BaseResponse>(HubMethod.Twinning, input);
            if (Plugin.DeveloperMode || response.Success)
            {
                Plugin.NotificationManager.AddNotification(NotificationHelper.Success(
                    "Successfully twinned", string.Empty));
            }
            else
            {
                Plugin.NotificationManager.AddNotification(NotificationHelper.Warning(
                    "Unable to twin", response.Message));
            }
        }
        catch (Exception e)
        {
            Plugin.Log.Warning($"Unable to twin, {e.Message}");
        }
    }
    
    /// <summary>
    ///     Used by the UI to determine the friends who are currently selected that you do not have permissions for
    /// </summary>
    public List<string> GetFriendsLackingPermissions()
    {
        var thoseWhoYouLackPermissionsFor = new List<string>();
        foreach (var selected in friendsListService.Selected)
        {
            if (selected.PermissionsGrantedByFriend.Has(PrimaryPermissions.Twinning) is false)
            {
                thoseWhoYouLackPermissionsFor.Add(selected.NoteOrFriendCode);
                continue;
            }
            
            if (SwapMods && selected.PermissionsGrantedByFriend.Has(PrimaryPermissions.Mods) is false)
            {
                thoseWhoYouLackPermissionsFor.Add(selected.NoteOrFriendCode);
            }
        }
        
        return thoseWhoYouLackPermissionsFor;
    }
}