using System;
using System.Collections.Generic;
using System.Linq;
using AetherRemoteClient.Ipc;
using AetherRemoteClient.Services;
using AetherRemoteClient.Utils;
using AetherRemoteCommon.Domain.Enums;
using AetherRemoteCommon.Domain.Enums.Permissions;
using AetherRemoteCommon.Domain.Network;
using AetherRemoteCommon.V2.Domain.Network;
using AetherRemoteCommon.V2.Domain.Network.Transform;
using ImGuiNET;

namespace AetherRemoteClient.UI.Views.Transformation;

/// <summary>
///     Handles events from the <see cref="TransformationViewUi"/>
/// </summary>
public class TransformationViewUiController(
    FriendsListService friendsListService,
    NetworkService networkService,
    GlamourerIpc glamourer)
{
    public string GlamourerCode = string.Empty;
    
    public bool ApplyCustomization = true;
    public bool ApplyEquipment = false;

    /// <summary>
    ///     Used to determine if all selected friends have permissions
    /// </summary>
    public PrimaryPermissions2 SelectedApplyTypePermissions = PrimaryPermissions2.GlamourerCustomization;
    
    /// <summary>
    ///     Sets <see cref="GlamourerCode"/> to your own glamourer code
    /// </summary>
    public async void CopyOwnGlamourer()
    {
        try
        {
            var result = await glamourer.GetDesignAsync();
            if (result is null)
            {
                NotificationHelper.Warning("Unable to get glamourer data",
                    "Unable to get your glamourer data at the moment.");
                return;
            }

            GlamourerCode = result;
        }
        catch (Exception e)
        {
            Plugin.Log.Warning($"Unable to copy own glamourer, {e.Message}");
        }
    }
    
    /// <summary>
    ///     Sets <see cref="GlamourerCode"/> to your target's glamourer code
    /// </summary>
    public async void CopyTargetGlamourer()
    {
        try
        {
            var index = await Plugin.RunOnFramework(() => Plugin.TargetManager.Target?.ObjectIndex);
            if (index is null)
            {
                NotificationHelper.Warning("No target",
                    "Unable to get target glamourer data because you don't have a target.");
                return;
            }
        
            var data = await glamourer.GetDesignAsync(index.Value);
            if (data is null)
            {
                NotificationHelper.Warning("Unable to get glamourer data",
                    "Unable to get target glamourer data at the moment.");
                return;
            }
        
            GlamourerCode = data;
        }
        catch (Exception e)
        {
            Plugin.Log.Warning($"Unable to copy target glamourer, {e.Message}");
        }
    }

    /// <summary>
    ///     Handles the transform from the Ui
    /// </summary>
    public async void Transform()
    {
        try
        {
            if (GlamourerCode.Length is 0)
                return;

            var applyType = GlamourerApplyFlags.Once
                            | (ApplyCustomization ? GlamourerApplyFlags.Customization : 0)
                            | (ApplyEquipment ? GlamourerApplyFlags.Equipment : 0);
            
            if (applyType is GlamourerApplyFlags.Once)
                applyType = GlamourerApplyFlags.All;
            
            var input = new TransformRequest
            {
                TargetFriendCodes = friendsListService.Selected.Select(friend => friend.FriendCode).ToList(),
                GlamourerData = GlamourerCode,
                GlamourerApplyType = applyType
            };
            
            var response = await networkService.InvokeAsync<ActionResponse>(HubMethod.Transform, input);
            ActionResponseParser.Parse("Transformation", response);
        }
        catch (Exception e)
        {
            Plugin.Log.Warning($"Unable to transform, {e.Message}");
        }
    }

    /// <summary>
    ///     Sets <see cref="GlamourerCode"/> to whatever is in your clipboard
    /// </summary>
    public void CopyFromClipboard()
    {
        GlamourerCode = ImGui.GetClipboardText();
    }
    
    /// <summary>
    ///     Gets a list of friend codes or notes of the people who you lack permissions to send to
    /// </summary>
    /// <returns></returns>
    public List<string> GetFriendsLackingPermissions()
    {
        var thoseWhoYouLackPermissionsFor = new List<string>();
        foreach (var selected in friendsListService.Selected)
        {
            if ((selected.PermissionsGrantedByFriend.Primary & SelectedApplyTypePermissions) != SelectedApplyTypePermissions)
                thoseWhoYouLackPermissionsFor.Add(selected.NoteOrFriendCode);
        }
        
        return thoseWhoYouLackPermissionsFor;
    }
}