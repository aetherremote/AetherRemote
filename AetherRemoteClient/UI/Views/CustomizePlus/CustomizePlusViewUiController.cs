using System;
using System.Collections.Generic;
using System.Linq;
using AetherRemoteClient.Services;
using AetherRemoteClient.Utils;
using AetherRemoteCommon.Domain.Enums;
using AetherRemoteCommon.Domain.Network;

namespace AetherRemoteClient.UI.Views.CustomizePlus;

public class CustomizePlusViewUiController(FriendsListService friendsListService, NetworkService networkService)
{
    /// <summary>
    ///     Customize+ data to send
    /// </summary>
    public string Customize = string.Empty;
    
    public async void SendCustomize()
    {
        try
        {
            if (Customize.Length is 0)
                return;

            var input = new CustomizePlusRequest
            {
                TargetFriendCodes = friendsListService.Selected.Select(friend => friend.FriendCode).ToList(),
                Customize = Customize
            };

            var response = await networkService.InvokeAsync<BaseResponse>(HubMethod.CustomizePlus, input);
            if (response.Success)
            {
                Customize = string.Empty;
                
                Plugin.NotificationManager.AddNotification(NotificationHelper.Success(
                    "Successfully applied customize plus template", string.Empty));
            }
            else
            {
                Plugin.NotificationManager.AddNotification(NotificationHelper.Warning(
                    "Unable to apply customize plus template", response.Message));
            }
        }
        catch (Exception e)
        {
            Plugin.Log.Warning($"Failed to apply customize plus template, {e.Message}");
        }
    }
    
    /// <summary>
    ///     Calculates the friends who you lack correct permissions to send to
    /// </summary>
    /// <returns></returns>
    public List<string> GetFriendsLackingPermissions()
    {
        var thoseWhoYouLackPermissionsFor = new List<string>();
        foreach (var selected in friendsListService.Selected)
        {
            if (selected.PermissionsGrantedByFriend.Primary.HasFlag(PrimaryPermissions.CustomizePlus) is false)
                thoseWhoYouLackPermissionsFor.Add(selected.NoteOrFriendCode);
        }
        return thoseWhoYouLackPermissionsFor;
    }
}