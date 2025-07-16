using System;
using System.Collections.Generic;
using System.Linq;
using AetherRemoteClient.Managers;
using AetherRemoteClient.Services;
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
    FriendsListService friendsListService,
    NetworkService networkService,
    ModManager modManager,
    PermanentTransformationManager permanentTransformationManager)
{
    public bool IncludeSelfInSwap;
    public bool SwapMods;
    public bool SwapMoodles;
    public bool SwapCustomizePlus;
    
    public bool PermanentTransformation = false;
    public string UnlockPin = string.Empty;
    
    /// <summary>
    ///     Used to determine if all selected friends have permissions
    /// </summary>
    public PrimaryPermissions2 SelectedAttributesPermissions = PrimaryPermissions2.BodySwap;

    /// <summary>
    ///     Handles the swap button from the Ui
    /// </summary>
    public async void Swap()
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
            
            var request = new BodySwapRequest
            {
                TargetFriendCodes = friendsListService.Selected.Select(friend => friend.FriendCode).ToList(),
                SwapAttributes = attributes,
                SenderCharacterName = IncludeSelfInSwap ? player.Name.ToString() : null,
                LockCode = PermanentTransformation ? UnlockPin : null
            };

            NotificationHelper.Info("Beginning body swap, this will take a moment", string.Empty);

            var response = await networkService.InvokeAsync<BodySwapResponse>(HubMethod.BodySwap, request);
            if (response.Result is ActionResponseEc.Success)
            {
                if (response.CharacterName is null)
                {
                    // We requested to be in the swap
                    if (request.SenderCharacterName is not null)
                    {
                        // Issue
                        Plugin.Log.Warning("[Body Swap] Expected a body in the response but none was present");
                    }
                }
                else
                {
                    // Actually apply glamourer, mods, etc...
                    if (await modManager.Assimilate(response.CharacterName, attributes).ConfigureAwait(false) is { } permanentTransformationData)
                    {
                        // If there is a lock code present, attempt to lock
                        if (request.LockCode is not null)
                        {
                            permanentTransformationData.UnlockCode = request.LockCode;
                            await permanentTransformationManager.Lock(permanentTransformationData);
                        }
                    }
                        
                    // Set your new identity
                    identityService.Identity = response.CharacterName;
                }
            }
            
            ActionResponseParser.Parse("Body Swap", response);
        }
        catch (Exception e)
        {
            Plugin.Log.Warning($"Unable to swap bodies, {e.Message}");
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