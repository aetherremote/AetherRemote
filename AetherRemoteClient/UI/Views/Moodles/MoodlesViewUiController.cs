using System;
using System.Collections.Generic;
using System.Linq;
using AetherRemoteClient.Services;
using AetherRemoteClient.Utils;
using AetherRemoteCommon.Domain.Enums;
using AetherRemoteCommon.Domain.Enums.Permissions;
using AetherRemoteCommon.Domain.Network;
using AetherRemoteCommon.V2.Domain.Network;
using AetherRemoteCommon.V2.Domain.Network.Moodles;

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

            var response = await networkService.InvokeAsync<ActionResponse>(HubMethod.Moodles, input);
            if (response.Result is ActionResponseEc.Success)
                Moodle = string.Empty;
            
            ActionResponseParser.Parse("Moodles", response);
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
            if ((selected.PermissionsGrantedByFriend.Primary & PrimaryPermissions2.Moodles) != PrimaryPermissions2.Moodles)
                thoseWhoYouLackPermissionsFor.Add(selected.NoteOrFriendCode);
        }
        return thoseWhoYouLackPermissionsFor;
    }
}