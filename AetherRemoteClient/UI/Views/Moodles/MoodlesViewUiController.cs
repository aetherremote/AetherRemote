using System;
using System.Collections.Generic;
using System.Linq;
using AetherRemoteClient.Services;
using AetherRemoteClient.Utils;
using AetherRemoteCommon.Domain.Enums;
using AetherRemoteCommon.Domain.Network;

namespace AetherRemoteClient.UI.Views.Moodles;

public class MoodlesViewUiController(FriendsListService friendsListService, NetworkService networkService)
{
    public string Moodle = string.Empty;
    
    public async void SendMoodle()
    {
        try
        {
            if (Moodle.Length is 0)
                return;

            var input = new MoodlesRequest
            {
                TargetFriendCodes = friendsListService.Selected.Select(friend => friend.FriendCode).ToList(),
                Moodle = Moodle
            };

            var response = await networkService.InvokeAsync<BaseResponse>(HubMethod.Moodles, input);
            if (response.Success)
            {
                Moodle = string.Empty;
                
                Plugin.NotificationManager.AddNotification(NotificationHelper.Success(
                    "Successfully added moodle", string.Empty));
            }
            else
            {
                Plugin.NotificationManager.AddNotification(NotificationHelper.Warning(
                    "Unable to add moodle", response.Message));
            }
        }
        catch (Exception e)
        {
            Plugin.Log.Warning($"Failed to add moodle, {e.Message}");
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
            if (selected.PermissionsGrantedByFriend.Primary.HasFlag(PrimaryPermissions.Moodles) is false)
                thoseWhoYouLackPermissionsFor.Add(selected.NoteOrFriendCode);
        }
        return thoseWhoYouLackPermissionsFor;
    }
}