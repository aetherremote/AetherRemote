using System;
using AetherRemoteClient.Managers;
using AetherRemoteClient.Services;
using AetherRemoteClient.UI.Components.Input;
using AetherRemoteClient.Utils;
using AetherRemoteCommon.Domain.Enums;
using AetherRemoteCommon.Domain.Enums.Permissions;
using AetherRemoteCommon.Domain.Network;
using AetherRemoteCommon.Domain.Network.Twinning;

namespace AetherRemoteClient.UI.Views.Twinning;

public class TwinningViewUiController(NetworkService network, SelectionManager selection)
{
    // Swap Parameters
    public bool SwapMods;
    public bool SwapMoodles;
    public bool SwapCustomizePlus;
    
    // Permanent Swappies
    public bool PermanentTransformation = false;
    public readonly FourDigitInput PinInput = new("TransformationInput");

    /// <summary>
    ///     Initiate a twinning request
    /// </summary>
    public async void Twin()
    {
        try
        {
            var attributes = CharacterAttributes.None;
            if (SwapMods) attributes |= CharacterAttributes.Mods;
            if (SwapMoodles) attributes |= CharacterAttributes.Moodles;
            if (SwapCustomizePlus) attributes |= CharacterAttributes.CustomizePlus;
            
            // Get the local player name
            if (Plugin.ObjectTable.LocalPlayer?.Name.TextValue is not { } playerName)
                return;

            // Create the request
            var request = new TwinningRequest(selection.GetSelectedFriendCodes(), playerName, attributes, null);
            
            // Invoke on the server
            var response = await network.InvokeAsync<ActionResponse>(HubMethod.Twinning, request);
            
            // Process the results
            ActionResponseParser.Parse("Twinning", response);
        }
        catch (Exception)
        {
            // Ignored
        }
    }
    
    /// <summary>
    ///     Used by the UI to determine the friends who are currently selected that you do not have permissions for
    /// </summary>
    public bool MissingPermissionsForATarget()
    {
        var attributes = PrimaryPermissions2.Twinning;
        if (SwapMods) attributes |= PrimaryPermissions2.Mods;
        if (SwapMoodles) attributes |= PrimaryPermissions2.Moodles;
        if (SwapCustomizePlus) attributes |= PrimaryPermissions2.CustomizePlus;
        
        foreach (var friend in selection.Selected)
            if ((friend.PermissionsGrantedByFriend.Primary & attributes) != attributes)
                return true;

        return false;
    }
}