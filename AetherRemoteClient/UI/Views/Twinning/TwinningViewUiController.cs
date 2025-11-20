using System;
using System.Collections.Generic;
using System.Linq;
using AetherRemoteClient.Managers;
using AetherRemoteClient.Services;
using AetherRemoteClient.UI.Components.Input;
using AetherRemoteClient.Utils;
using AetherRemoteCommon.Domain.Enums;
using AetherRemoteCommon.Domain.Enums.Permissions;
using AetherRemoteCommon.Domain.Network;
using AetherRemoteCommon.Domain.Network.Twinning;

namespace AetherRemoteClient.UI.Views.Twinning;

public class TwinningViewUiController(NetworkService networkService, SelectionManager selectionManager)
{
    public bool SwapMods;
    public bool SwapMoodles;
    public bool SwapCustomizePlus;
    public bool PermanentTransformation = false;
    public readonly FourDigitInput PinInput = new("TransformationInput");

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
            
            var request = new TwinningRequest
            {
                TargetFriendCodes = selectionManager.GetSelectedFriendCodes(),
                SwapAttributes = attributes,
                CharacterName = player.Name.ToString()
            };
            
            if (PermanentTransformation)
            {
                var pin = PinInput.Value;
                if (pin.Length is 4)
                {
                    request.LockCode = pin;
                }
                else
                {
                    Plugin.Log.Warning("[TwinningViewUiController] Pin is not 4 characters");
                    return;
                }
            }

            var response = await networkService.InvokeAsync<ActionResponse>(HubMethod.Twinning, request);
            ActionResponseParser.Parse("Twinning", response);
        }
        catch (Exception e)
        {
            Plugin.Log.Warning($"Unable to twin, {e.Message}");
        }
    }
    
    public bool AllSelectedTargetsHaveElevatedPermissions()
    {
        return selectionManager.Selected.All(friend =>
            (friend.PermissionsGrantedByFriend.Elevated & ElevatedPermissions.PermanentTransformation) ==
            ElevatedPermissions.PermanentTransformation);
    }
    
    /// <summary>
    ///     Used by the UI to determine the friends who are currently selected that you do not have permissions for
    /// </summary>
    public List<string> GetFriendsLackingPermissions()
    {
        var thoseWhoYouLackPermissionsFor = new List<string>();
        foreach (var selected in selectionManager.Selected)
        {
            if ((selected.PermissionsGrantedByFriend.Primary & SelectedAttributesPermissions) != SelectedAttributesPermissions)
                thoseWhoYouLackPermissionsFor.Add(selected.NoteOrFriendCode);
        }
        
        return thoseWhoYouLackPermissionsFor;
    }
}