using System;
using System.Collections.Generic;
using System.Linq;
using AetherRemoteClient.Dependencies.Moodles.Domain;
using AetherRemoteClient.Dependencies.Moodles.Services;
using AetherRemoteClient.Managers;
using AetherRemoteClient.Services;
using AetherRemoteClient.Utils;
using AetherRemoteCommon.Domain.Enums.Permissions;
using AetherRemoteCommon.Domain.Network;
using AetherRemoteCommon.Domain.Network.Moodles;
using Dalamud.Interface.Textures;
using Dalamud.Interface.Textures.TextureWraps;

namespace AetherRemoteClient.UI.Views.Moodles;

public class MoodlesViewUiController
{
    // Instantiate
    private readonly CommandLockoutService _commandLockoutService;
    private readonly MoodlesService _moodlesService;
    private readonly NetworkService _networkService;
    private readonly SelectionManager _selectionManager;

    public MoodlesViewUiController(CommandLockoutService commandLockoutService, NetworkService networkService, MoodlesService moodlesService, SelectionManager selectionManager)
    {
        _commandLockoutService =  commandLockoutService;
        _networkService = networkService;
        _moodlesService = moodlesService;
        _selectionManager = selectionManager;

        RefreshMoodles();
    }

    /// <summary>
    ///     Word to narrow down a search for a specific Moodle
    /// </summary>
    public string SearchTerm = string.Empty;

    /// <summary>
    ///     The list of moodles available
    /// </summary>
    private List<Moodle> _moodles = [];
    
    /// <summary>
    ///     A filtered list of moodles based on search term
    /// </summary>
    public List<Moodle> FilteredMoodles => _moodles.Where(moodle => moodle.PrettyTitle.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase)).ToList();

    /// <summary>
    ///     The current index of the selected Moodle, -1 if none selected
    /// </summary>
    public int SelectedMoodleIndex = -1;
    
    /// <summary>
    ///     Attempts to get the image asset. Implements caching to ease burden of searching / loading images
    /// </summary>
    public static IDalamudTextureWrap? TryGetIcon(int iconId)
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
    public async void RefreshMoodles()
    {
        try
        {
            // Reset index
            SelectedMoodleIndex = -1;
            
            // Request all the Moodles again
            _moodles = await _moodlesService.GetMoodles().ConfigureAwait(false);
        }
        catch (Exception)
        {
            // ignored
        }
    }
    
    public async void SendMoodle()
    {
        try
        {
            if (SelectedMoodleIndex < 0)
                return;
            
            _commandLockoutService.Lock();
            
            var moodle = FilteredMoodles[SelectedMoodleIndex];
            var request = new MoodlesRequest(_selectionManager.GetSelectedFriendCodes(), moodle.Info);
            var response = await _networkService.InvokeAsync<ActionResponse>(HubMethod.Moodles, request).ConfigureAwait(false);
            
            ActionResponseParser.Parse("Moodles", response);
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
    public List<string> GetFriendsLackingPermissions()
    {
        var thoseWhoYouLackPermissionsFor = new List<string>();
        foreach (var selected in _selectionManager.Selected)
        {
            if ((selected.PermissionsGrantedByFriend.Primary & PrimaryPermissions2.Moodles) != PrimaryPermissions2.Moodles)
                thoseWhoYouLackPermissionsFor.Add(selected.NoteOrFriendCode);
        }
        return thoseWhoYouLackPermissionsFor;
    }
}