using System;
using System.Collections.Generic;
using System.Linq;
using AetherRemoteClient.Services;
using AetherRemoteClient.Utils;
using AetherRemoteCommon.Domain.Enums;
using AetherRemoteCommon.Domain.Enums.Permissions;
using AetherRemoteCommon.Domain.Network;
using AetherRemoteCommon.Domain.Network.Twinning;

namespace AetherRemoteClient.UI.Views.Twinning;

public class TwinningViewUiController(FriendsListService friendsListService, NetworkService networkService)
{
    public bool SwapMods;
    public bool SwapMoodles;
    public bool SwapCustomizePlus;
    
    public bool PermanentTransformation = false;
    public string UnlockPin = string.Empty;

    /// <summary>
    ///     Used to determine if all selected friends have permissions
    /// </summary>
    public PrimaryPermissions2 SelectedAttributesPermissions = PrimaryPermissions2.Twinning;

    public async void Twin()
    {
        try
        {
            if (Plugin.ClientState.LocalPlayer is not { } player)
                return;

            var attributes = CharacterAttributes.None;
            if (SwapMods)
                attributes |= CharacterAttributes.Mods;
            if (SwapMoodles)
                attributes |= CharacterAttributes.Moodles;
            if (SwapCustomizePlus)
                attributes |= CharacterAttributes.CustomizePlus;
            
            var input = new TwinningRequest
            {
                TargetFriendCodes = friendsListService.Selected.Select(friend => friend.FriendCode).ToList(),
                SwapAttributes = attributes,
                CharacterName = player.Name.ToString(),
                LockCode = PermanentTransformation ? UnlockPin : null
            };

            var response = await networkService.InvokeAsync<ActionResponse>(HubMethod.Twinning, input);
            ActionResponseParser.Parse("Twinning", response);
        }
        catch (Exception e)
        {
            Plugin.Log.Warning($"Unable to twin, {e.Message}");
        }
    }
    
    public bool AllSelectedTargetsHaveElevatedPermissions()
    {
        return friendsListService.Selected.All(friend =>
            (friend.PermissionsGrantedByFriend.Elevated & ElevatedPermissions.PermanentTransformation) ==
            ElevatedPermissions.PermanentTransformation);
    }
    
    /// <summary>
    ///     Used by the UI to determine the friends who are currently selected that you do not have permissions for
    /// </summary>
    public List<string> GetFriendsLackingPermissions()
    {
        var thoseWhoYouLackPermissionsFor = new List<string>();
        foreach (var selected in friendsListService.Selected)
        {
            if ((selected.PermissionsGrantedByFriend.Primary & SelectedAttributesPermissions) != SelectedAttributesPermissions)
                thoseWhoYouLackPermissionsFor.Add(selected.NoteOrFriendCode);
        }
        
        return thoseWhoYouLackPermissionsFor;
    }
}