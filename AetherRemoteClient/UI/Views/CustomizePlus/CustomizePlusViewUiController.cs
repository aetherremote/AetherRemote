using System;
using System.Collections.Generic;
using AetherRemoteClient.Services;
using AetherRemoteClient.Utils;
using AetherRemoteCommon.Domain.Enums;
using AetherRemoteCommon.Domain.Network;
using AetherRemoteCommon.V2.Domain.Enum;
using AetherRemoteCommon.V2.Domain.Network;
using AetherRemoteCommon.V2.Domain.Network.Customize;

namespace AetherRemoteClient.UI.Views.CustomizePlus;

public class CustomizePlusViewUiController(FriendsListService friendsListService, NetworkService networkService)
{
    /// <summary>
    ///     Customize+ data to send
    /// </summary>
    public string CustomizeData = string.Empty;
    
    public async void SendCustomize()
    {
        try
        {
            if (CustomizeData.Length is 0)
                return;
            
            var request = new CustomizeRequest(friendsListService.SelectedFriendCodes, CustomizeData);
            var response = await networkService.InvokeAsync<ActionResponse>(HubMethod.CustomizePlus, request).ConfigureAwait(false);
            if (response.Result is ActionResponseEc.Success)
                CustomizeData = string.Empty;
            
            ActionResultParser.Parse("Customize", response);
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
            if (selected.PermissionsGrantedByFriend.Primary.HasFlag(PrimaryPermissions.Customize) is false)
                thoseWhoYouLackPermissionsFor.Add(selected.NoteOrFriendCode);
        }
        return thoseWhoYouLackPermissionsFor;
    }
}