using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AetherRemoteClient.Domain.Moodles;
using AetherRemoteCommon.Domain.Enums.Permissions;
using AetherRemoteCommon.Domain.Network;
using AetherRemoteCommon.Network.Domain;
using AetherRemoteCommon.Network.Domain.Payloads;
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
    private async Task RefreshMoodles()
    {
        // Reset index
        _selectedMoodleIndex = -1;
            
        // Request all the Moodles again
        _moodles = await _moodlesService.GetMoodles().ConfigureAwait(false) ?? [];
    }

    private async Task SendMoodle()
    {
        if (_selectedMoodleIndex < 0)
            return;
        
        var targets = _selectionManager.GetSelectedFriendCodes();
        if (targets.Count is 0)
            return;
        
        _commandLockoutService.Lock();
        var payload = new MoodlesPayload(FilteredMoodles[_selectedMoodleIndex].Info);
        await _networkRequestManager.Send<MoodlesPayload, NoPayload>(targets, HubMethod.Moodles, payload).ConfigureAwait(false);
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
        _ = RefreshMoodles().ConfigureAwait(false);
    }
}