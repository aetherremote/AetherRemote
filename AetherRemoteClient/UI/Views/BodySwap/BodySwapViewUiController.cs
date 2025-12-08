using System;
using AetherRemoteClient.Domain;
using AetherRemoteClient.Managers;
using AetherRemoteClient.Services;
using AetherRemoteClient.UI.Components.Input;
using AetherRemoteClient.Utils;
using AetherRemoteCommon.Domain.Enums;
using AetherRemoteCommon.Domain.Enums.Permissions;
using AetherRemoteCommon.Domain.Network;
using AetherRemoteCommon.Domain.Network.BodySwap;

namespace AetherRemoteClient.UI.Views.BodySwap;

/// <summary>
///     Handles events from the <see cref="BodySwapViewUi"/>
/// </summary>
public class BodySwapViewUiController(
    IdentityService identityService,
    NetworkService networkService,
    CharacterTransformationManager characterTransformationManager,
    SelectionManager selectionManager)
{
    // Swap Parameters
    public bool SwapMods;
    public bool SwapMoodles;
    public bool SwapCustomizePlus;
    
    // Permanent Swappies
    public bool PermanentTransformation = false;
    public readonly FourDigitInput PinInput = new("TransformationInput");

    /// <summary>
    ///     Initiates a body swap command
    /// </summary>
    public void SwapBodies()
    {
        var attributes = CharacterAttributes.None;
        if (SwapMods) attributes |= CharacterAttributes.Mods;
        if (SwapMoodles) attributes |= CharacterAttributes.Moodles;
        if (SwapCustomizePlus) attributes |= CharacterAttributes.CustomizePlus;
        
        BodySwap(new BodySwapRequest(selectionManager.GetSelectedFriendCodes(), attributes, null, null));
    }

    /// <summary>
    ///     Initiates a body swap command but also with the sender as one of the participants in the body swap
    /// </summary>
    public void SwapBodiesIncludingSelf()
    {
        var attributes = CharacterAttributes.None;
        if (SwapMods) attributes |= CharacterAttributes.Mods;
        if (SwapMoodles) attributes |= CharacterAttributes.Moodles;
        if (SwapCustomizePlus) attributes |= CharacterAttributes.CustomizePlus;

        if (Plugin.ObjectTable.LocalPlayer?.Name.TextValue is not { } playerName)
            return;
        
        BodySwap(new BodySwapRequest(selectionManager.GetSelectedFriendCodes(), attributes, playerName, null));
    }

    private async void BodySwap(BodySwapRequest request)
    {
        try
        {
            // Invoke on the server
            var response = await networkService.InvokeAsync<BodySwapResponse>(HubMethod.BodySwap, request);
            if (response.Result is not ActionResponseEc.Success)
            {
                ActionResponseParser.Parse("Body Swap", response);
                return;
            }
            
            // If the character we'd be body swapping into was null...
            if (response.CharacterName is null)
            {
                // ...but we expected to get back a result by submitting our name in the body swap request...
                if (request.SenderCharacterName is not null)
                {
                    // ...exit and log the error
                    return;
                }
            }
            else
            {
                // Otherwise just body swap into them
                await characterTransformationManager.ApplyCharacterTransformation(response.CharacterName, request.SwapAttributes);
                
                // Mark us as altered
                identityService.AddAlteration(IdentityAlterationType.BodySwap, "You");
            }
            
            // Process the results
            ActionResponseParser.Parse("Body Swap", response);
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
        var attributes = PrimaryPermissions2.BodySwap;
        if (SwapMods) attributes |= PrimaryPermissions2.Mods;
        if (SwapMoodles) attributes |= PrimaryPermissions2.Moodles;
        if (SwapCustomizePlus) attributes |= PrimaryPermissions2.CustomizePlus;
        
        foreach (var friend in selectionManager.Selected)
            if ((friend.PermissionsGrantedByFriend.Primary & attributes) != attributes)
                return true;

        return false;
    }
}