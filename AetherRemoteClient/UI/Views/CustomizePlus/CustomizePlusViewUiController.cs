using System;
using System.Collections.Generic;
using AetherRemoteClient.Dependencies.CustomizePlus.Services;
using AetherRemoteClient.Managers;
using AetherRemoteClient.Services;
using AetherRemoteClient.Utils;
using AetherRemoteCommon.Domain.Enums;
using AetherRemoteCommon.Domain.Enums.Permissions;
using AetherRemoteCommon.Domain.Network;
using AetherRemoteCommon.Domain.Network.Customize;

namespace AetherRemoteClient.UI.Views.CustomizePlus;

public class CustomizePlusViewUiController(CustomizePlusService customizePlusService, NetworkService networkService, SelectionManager selectionManager)
{
    /// <summary>
    ///     Customize+ data to send
    /// </summary>
    public string SearchTerm = string.Empty;
    
    public async void SendCustomizeProfile()
    {
        
    }

    public async void RefreshCustomizeProfiles()
    {
        var list = customizePlusService.GetProfiles();
    }
    
    /// <summary>
    ///     Calculates the friends who you lack correct permissions to send to
    /// </summary>
    /// <returns></returns>
    public List<string> GetFriendsLackingPermissions()
    {
        var thoseWhoYouLackPermissionsFor = new List<string>();
        foreach (var selected in selectionManager.Selected)
        {
            if ((selected.PermissionsGrantedByFriend.Primary & PrimaryPermissions2.CustomizePlus) != PrimaryPermissions2.CustomizePlus)
                thoseWhoYouLackPermissionsFor.Add(selected.NoteOrFriendCode);
        }
        return thoseWhoYouLackPermissionsFor;
    }
}