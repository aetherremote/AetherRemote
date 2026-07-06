using System;
using System.Collections.Generic;
using System.Linq;
using AetherRemoteClient.Domain.Moodles;
using AetherRemoteClient.Utils;
using AetherRemoteCommon.Domain.Enums.Permissions;
using AetherRemoteCommon.Domain.Network;
using AetherRemoteCommon.Domain.Network.Moodles;
using Dalamud.Interface.Textures;
using Dalamud.Interface.Textures.TextureWraps;

namespace AetherRemoteClient.UI.Views.Moodles;

public partial class MoodlesView
{
    /// <summary>
    ///     Word to narrow down a search for a specific Moodle
    /// </summary>
    private string _searchTerm = string.Empty;

    /// <summary>
    ///     The list of moodles available
    /// </summary>
    private List<Moodle> _moodles = [];
    
    /// <summary>
    ///     A filtered list of moodles based on search term
    /// </summary>
    private List<Moodle> FilteredMoodles => _moodles.Where(moodle => moodle.PrettyTitle.Contains(_searchTerm, StringComparison.OrdinalIgnoreCase)).ToList();

    /// <summary>
    ///     The current index of the selected Moodle, -1 if none selected
    /// </summary>
    private int _selectedMoodleIndex = -1;
    
    /// <summary>
    ///     Attempts to get the image asset. Implements caching to ease burden of searching / loading images
    /// </summary>
    private static IDalamudTextureWrap? TryGetIcon(int iconId)
    {
        try
        {
            var texture = Plugin.TextureProvider.GetFromGameIcon(new GameIconLookup((uint)iconId));
            return texture.TryGetWrap(out var wrap, out _) ? wrap : null;
        }
        catch (Exception e)
        {
            Plugin.Log.Warning($"[MoodlesViewUiController.TryGetIcon] Unexpectedly failed to get Moodle icon, {e}");
            return null;
        }
    }

    /// <summary>
    ///     Refreshes the available moodles
    /// </summary>
    private async void RefreshMoodles()
    {
        try
        {
            // Reset index
            _selectedMoodleIndex = -1;
            
            // Request all the Moodles again
            _moodles = await _moodlesService.GetMoodles().ConfigureAwait(false) ?? [];
        }
        catch (Exception)
        {
            // ignored
        }
    }

    private async void SendMoodle()
    {
        try
        {
            if (_selectedMoodleIndex < 0)
                return;
            
            _commandLockoutService.Lock();
            
            var moodle = FilteredMoodles[_selectedMoodleIndex];
            var request = new MoodlesRequest(_selectionManager.GetSelectedFriendCodes(), moodle.Info);
            var response = await _networkService.InvokeAsync<ActionResponse>(HubMethod.Moodles, request).ConfigureAwait(false);
            
            ActionResponseParser.Parse("Moodles", response, []); // TODO: Fix []
        }
        catch (Exception e)
        {
            Plugin.Log.Warning($"Failed to add moodle, {e.Message}");
        }
    }
    
    /// <summary>
    ///     Calculates the friends who you lack correct permissions to send to
    /// </summary>
    /// <returns></returns>
    private bool MissingPermissionsForATarget()
    {
        foreach (var friend in _selectionManager.Selected)
        {
            if (friend.PermissionsGrantedByFriend is null)
                continue;
            
            if ((friend.PermissionsGrantedByFriend.Primary & PrimaryPermissions.Moodles) is not PrimaryPermissions.Moodles)
                return true;
        }

        return false;
    }
    
    private void OnIpcReady(object? sender, EventArgs e)
    {
        RefreshMoodles();
    }
}