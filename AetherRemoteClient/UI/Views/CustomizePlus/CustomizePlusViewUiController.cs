using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AetherRemoteClient.Dependencies.CustomizePlus.Domain;
using AetherRemoteClient.Dependencies.CustomizePlus.Services;
using AetherRemoteClient.Domain;
using AetherRemoteClient.Managers;
using AetherRemoteClient.Services;
using AetherRemoteClient.Utils;
using AetherRemoteCommon.Domain.Enums.Permissions;
using AetherRemoteCommon.Domain.Network;
using AetherRemoteCommon.Domain.Network.Customize;

namespace AetherRemoteClient.UI.Views.CustomizePlus;

/// <summary>
///     Ui Controller
/// </summary>
public class CustomizePlusViewUiController : IDisposable
{
    // Injected
    private readonly CustomizePlusService _customizePlusService;
    private readonly NetworkCommandManager _networkCommandManager;
    private readonly SelectionManager _selectionManager;
    
    /// <summary>
    ///     Search for the profile we'd like to send
    /// </summary>
    public string SearchTerm = string.Empty;
    
    /// <summary>
    ///     The currently selected Guid of the Profile to send
    /// </summary>
    public Guid SelectedProfileId = Guid.Empty;
    
    /// <summary>
    ///     Cached list of profiles
    /// </summary>
    private List<FolderNode<Profile>>? _sorted;
    
    /// <summary>
    ///     Filtered cached list of profiles
    /// </summary>
    private List<FolderNode<Profile>>? _filtered;
    
    /// <summary>
    ///     The profiles to display in the Ui
    /// </summary>
    public List<FolderNode<Profile>>? Profiles => SearchTerm == string.Empty ? _sorted : _filtered;

    /// <summary>
    ///     Should this profile be added on top of existing ones?
    /// </summary>
    public bool ShouldApplyAsAdditive = false;
    
    /// <summary>
    ///     <inheritdoc cref="CustomizePlusViewUiController"/>
    /// </summary>
    public CustomizePlusViewUiController(CustomizePlusService customizePlusService, NetworkCommandManager networkCommandManager, SelectionManager selectionManager)
    {
        _customizePlusService = customizePlusService;
        _networkCommandManager = networkCommandManager;
        _selectionManager = selectionManager;

        _customizePlusService.IpcReady += OnIpcReady;
        if (_customizePlusService.ApiAvailable)
            _ = RefreshCustomizeProfiles();
    }
    
    /// <summary>
    ///     Filters the sorted profile list by search term
    /// </summary>
    public void FilterProfilesBySearchTerm()
    {
        _filtered = _sorted is not null 
            ? FilterFolderNodes(_sorted, SearchTerm).ToList() 
            : null;
    }

    /// <summary>
    ///     Recursive method to filter nodes based on both folders and content names
    /// </summary>
    private List<FolderNode<Profile>> FilterFolderNodes(IEnumerable<FolderNode<Profile>> nodes, string searchTerms)
    {
        // Reset the selected so possibly unselected profiles aren't stored
        SelectedProfileId = Guid.Empty;
        
        // Iterate to determine what stays and what goes
        var results = new List<FolderNode<Profile>>();
        foreach (var node in nodes)
        {
            // The recursive part, filtering on the children to see if there were any matches
            var children = FilterFolderNodes(node.Children.Values, searchTerms).ToDictionary(n => n.Name);
            
            // Check if the item inside the folder's name matches
            var matches = node.Content is not null && node.Content.Name.Contains(searchTerms, StringComparison.OrdinalIgnoreCase);
            
            // If this is a folder with no children, exclude it
            if (matches is false && children.Count is 0)
                continue;
            
            // Add
            results.Add(new FolderNode<Profile>(node.Name, node.Content, children));
        }
        
        return results;
    }

    /// <summary>
    ///     Refresh the cache of available profiles
    /// </summary>
    public async Task RefreshCustomizeProfiles()
    {
        SelectedProfileId = Guid.Empty;

        _sorted = await _customizePlusService.GetProfiles().ConfigureAwait(false) is { } unsorted
            ? unsorted.Children.Values.ToList()
            : null;
    }
    
    /// <summary>
    ///     Calculates the friends who you lack correct permissions to send to
    /// </summary>
    public bool MissingPermissionsForATarget()
    {
        foreach (var friend in _selectionManager.Selected)
        {
            if (friend.PermissionsGrantedByFriend is null)
                continue;
            
            if ((friend.PermissionsGrantedByFriend.Primary & PrimaryPermissions.CustomizePlus) is not PrimaryPermissions.CustomizePlus)
                return true;
        }

        return false;
    }
    
    /// <summary>
    ///     Sends a request to the server
    /// </summary>
    public async Task SendCustomizeProfile()
    {
        if (SelectedProfileId == Guid.Empty)
            return;

        if (await _customizePlusService.GetProfile(SelectedProfileId).ConfigureAwait(false) is not { } profile)
            return;

        var bytes = Encoding.UTF8.GetBytes(profile);
        await _networkCommandManager.SendCustomize(_selectionManager.GetSelectedFriendCodes(), bytes, ShouldApplyAsAdditive).ConfigureAwait(false);
    }
    
    private void OnIpcReady(object? sender, EventArgs e)
    {
        _ = RefreshCustomizeProfiles();
    }

    public void Dispose()
    {
        _customizePlusService.IpcReady -= OnIpcReady;
        GC.SuppressFinalize(this);
    }
}