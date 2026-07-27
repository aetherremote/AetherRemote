using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AetherRemoteClient.Domain;
using AetherRemoteClient.Domain.CustomizePlus;
using AetherRemoteCommon.Domain.Enums;
using AetherRemoteCommon.Domain.Enums.Permissions;
using AetherRemoteCommon.Domain.Network;
using AetherRemoteCommon.Network.Domain;
using AetherRemoteCommon.Network.Domain.Payloads;

namespace AetherRemoteClient.UI.Views.CustomizePlus;

public partial class CustomizePlusView
{
    // Const
    private const int ApplyModeDefault = 0;
    private const int ApplyModeMerge = 1;
    
    /// <summary>
    ///     Search for the profile we'd like to send
    /// </summary>
    private string _searchTerm = string.Empty;
    
    /// <summary>
    ///     The currently selected Guid of the Profile to send
    /// </summary>
    private Guid _selectedProfileId = Guid.Empty;
    
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
    private List<FolderNode<Profile>>? Profiles => _searchTerm == string.Empty ? _sorted : _filtered;
    
    /// <summary>
    ///     The application mode of how a customize profile should be applied
    /// </summary>
    private int _applyMode;
    
    /// <summary>
    ///     Filters the sorted profile list by search term
    /// </summary>
    private void FilterProfilesBySearchTerm()
    {
        _filtered = _sorted is not null 
            ? FilterFolderNodes(_sorted, _searchTerm).ToList() 
            : null;
    }

    /// <summary>
    ///     Recursive method to filter nodes based on both folders and content names
    /// </summary>
    private List<FolderNode<Profile>> FilterFolderNodes(IEnumerable<FolderNode<Profile>> nodes, string searchTerms)
    {
        // Reset the selected so possibly unselected profiles aren't stored
        _selectedProfileId = Guid.Empty;
        
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
    private async Task RefreshCustomizeProfiles()
    {
        _selectedProfileId = Guid.Empty;

        if (await _customizePlusService.GetProfiles().ConfigureAwait(false) is not { } profiles)
            return;
        
        var root = new FolderNode<Profile>("Root", null);
        foreach (var profile in profiles)
        {
            var parts = profile.Path.Split('/', StringSplitOptions.RemoveEmptyEntries);
            var current = root;

            for (var i = 0; i < parts.Length; i++)
            {
                var part = parts[i];
                if (current.Children.TryGetValue(part, out var node) is false)
                {
                    node = new FolderNode<Profile>(part, i == parts.Length - 1 ? profile : null);
                    current.Children[part] = node;
                }
                
                current = node;
            }
        }
        
        // The dictionary provided by customize is not sorted
        FolderNode<Profile>.SortTree(root);
        
        // Assignment
        _sorted = root.Children.Values.ToList();
    }
    
    /// <summary>
    ///     Calculates the friends who you lack correct permissions to send to
    /// </summary>
    private bool MissingPermissionsForATarget()
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
    private async Task SendCustomizeProfile()
    {
        if (_selectedProfileId == Guid.Empty)
            return;

        if (await _customizePlusService.GetProfile(_selectedProfileId).ConfigureAwait(false) is not { } profile)
            return;
        
        var bytes = Encoding.UTF8.GetBytes(profile);
        var applyMode = _applyMode switch
        {
            ApplyModeDefault => CustomizeApplyMode.Default,
            ApplyModeMerge => CustomizeApplyMode.Merge,
            _ => CustomizeApplyMode.Default
        };

        var targets = _selectionManager.GetSelectedFriendCodes();
        if (targets.Count is 0)
            return;
        
        _commandLockoutService.Lock();
        var payload = new CustomizePlusPayload(bytes, applyMode);
        await _networkRequestManager.Send<CustomizePlusPayload, NoPayload>(targets, HubMethod.CustomizePlus, payload).ConfigureAwait(false);
    }
    
    private void OnIpcReady(object? sender, EventArgs e)
    {
        _ = RefreshCustomizeProfiles().ConfigureAwait(false);
    }
}