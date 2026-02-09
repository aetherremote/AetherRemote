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
    CommandLockoutService commandLockout,
    IdentityService identityService,
    NetworkService networkService,
    CharacterTransformationManager characterTransformationManager,
    SelectionManager selectionManager)
{
    // Swap Parameters
    public bool SwapMods;
    public bool SwapMoodles;
    public bool SwapCustomizePlus;
    public bool SwapHonorific;
    
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
        if (SwapHonorific) attributes |= CharacterAttributes.Honorific;

        BodySwap(new BodySwapRequest(selectionManager.GetSelectedFriendCodes(), null, null, attributes, null));
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
        if (SwapHonorific) attributes |= CharacterAttributes.Honorific;

        if (Plugin.CharacterConfiguration is not { } character)
            return;
        
        BodySwap(new BodySwapRequest(selectionManager.GetSelectedFriendCodes(), character.Name, character.World, attributes, null));
    }

    private async void BodySwap(BodySwapRequest request)
    {
        try
        {
            // Feedback Notification
            NotificationHelper.Info("Initiating body swap, this will take a moment", string.Empty);
            
            commandLockout.Lock();
            
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
        catch (Exception e)
        {
            Plugin.Log.Warning(e.ToString());
        }
    }
    
    /// <summary>
    ///     Used by the UI to determine the friends who are currently selected that you do not have permissions for
    /// </summary>
    public bool MissingPermissionsForATarget()
    {
        var attributes = PrimaryPermissions.BodySwap;
        if (SwapMods) attributes |= PrimaryPermissions.Mods;
        if (SwapMoodles) attributes |= PrimaryPermissions.Moodles;
        if (SwapCustomizePlus) attributes |= PrimaryPermissions.CustomizePlus;
        if (SwapHonorific) attributes |= PrimaryPermissions.Honorific;
        
        foreach (var friend in selectionManager.Selected)
        {
            if (friend.PermissionsGrantedByFriend is null)
                continue;
            
            if ((friend.PermissionsGrantedByFriend.Primary & attributes) != attributes)
                return true;
        }

        return false;
    }
}