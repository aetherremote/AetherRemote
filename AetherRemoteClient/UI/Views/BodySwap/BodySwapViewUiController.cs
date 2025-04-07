using System;
using System.Collections.Generic;
using System.Linq;
using AetherRemoteClient.Managers;
using AetherRemoteClient.Services;
using AetherRemoteClient.Utils;
using AetherRemoteCommon.Domain;
using AetherRemoteCommon.Domain.Enums;
using AetherRemoteCommon.Domain.Network;
using AetherRemoteCommon.Util;

namespace AetherRemoteClient.UI.Views.BodySwap;

/// <summary>
///     Handles events from the <see cref="BodySwapViewUi"/>
/// </summary>
public class BodySwapViewUiController(
    IdentityService identityService,
    FriendsListService friendsListService,
    NetworkService networkService,
    ModManager modManager)
{
    public bool IncludeSelfInSwap;
    public bool SwapMods;
    public bool SwapMoodles;
    public bool SwapCustomizePlus;

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
            
            var input = new BodySwapRequest
            {
                TargetFriendCodes = friendsListService.Selected.Select(friend => friend.FriendCode).ToList(),
                SwapAttributes = attributes,
                Identity = IncludeSelfInSwap
                    ? new CharacterIdentity
                    {
                        GameObjectName = player.Name.ToString(),
                        CharacterName = identityService.Identity
                    }
                    : null
            };

            Plugin.NotificationManager.AddNotification(
                NotificationHelper.Info("Beginning body swap, this may take a moment", string.Empty));

            var response =
                await networkService.InvokeAsync<BodySwapRequest, BodySwapResponse>(HubMethod.BodySwap, input);
            if (Plugin.DeveloperMode || response.Success)
            {
                if (response.Identity is null)
                {
                    Plugin.NotificationManager.AddNotification(NotificationHelper.Warning(
                        "Unable to swap bodies", "You did not receive a valid identity after the swap"));
                }
                else
                {
                    Plugin.NotificationManager.AddNotification(NotificationHelper.Success(
                        "Successfully swapped bodies", string.Empty));
                    
                    // Actually apply glamourer, mods, etc...
                    await modManager.Assimilate(response.Identity.GameObjectName, attributes).ConfigureAwait(false);
                    
                    // Set your new identity
                    identityService.Identity = response.Identity.CharacterName;
                }
            }
            else
            {
                Plugin.NotificationManager.AddNotification(NotificationHelper.Warning(
                    "Unable to swap bodies", response.Message));
            }
        }
        catch (Exception e)
        {
            Plugin.Log.Warning($"Unable to swap bodies, {e.Message}");
        }
    }
    
    /// <summary>
    ///     Used by the UI to determine the friends who are currently selected that you do not have permissions for
    /// </summary>
    public List<string> GetFriendsLackingPermissions()
    {
        var thoseWhoYouLackPermissionsFor = new List<string>();
        foreach (var selected in friendsListService.Selected)
        {
            if (selected.PermissionsGrantedByFriend.Has(PrimaryPermissions.BodySwap) is false)
            {
                thoseWhoYouLackPermissionsFor.Add(selected.NoteOrFriendCode);
                continue;
            }

            if (SwapMods && selected.PermissionsGrantedByFriend.Has(PrimaryPermissions.Mods) is false)
            {
                thoseWhoYouLackPermissionsFor.Add(selected.NoteOrFriendCode);
                continue;
            }

            if (SwapMoodles && selected.PermissionsGrantedByFriend.Has(PrimaryPermissions.Moodles) is false)
            {
                thoseWhoYouLackPermissionsFor.Add(selected.NoteOrFriendCode);
                continue;
            }
            
            if (SwapCustomizePlus && selected.PermissionsGrantedByFriend.Has(PrimaryPermissions.CustomizePlus) is false)
            {
                thoseWhoYouLackPermissionsFor.Add(selected.NoteOrFriendCode);
            }
        }
        
        return thoseWhoYouLackPermissionsFor;
    }
}