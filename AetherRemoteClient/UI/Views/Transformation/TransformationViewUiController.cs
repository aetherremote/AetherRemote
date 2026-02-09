using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AetherRemoteClient.Dependencies.Glamourer.Domain;
using AetherRemoteClient.Dependencies.Glamourer.Services;
using AetherRemoteClient.Domain;
using AetherRemoteClient.Managers;
using AetherRemoteClient.Services;
using AetherRemoteClient.Utils;
using AetherRemoteCommon.Domain.Enums;
using AetherRemoteCommon.Domain.Enums.Permissions;
using AetherRemoteCommon.Domain.Network;
using AetherRemoteCommon.Domain.Network.Transform;

namespace AetherRemoteClient.UI.Views.Transformation;

/// <summary>
///     Handles events from the <see cref="TransformationViewUi"/>
/// </summary>
public class TransformationViewUiController : IDisposable
{
    // Injected
    private readonly CommandLockoutService _commandLockoutService;
    private readonly GlamourerService _glamourerService;
    private readonly NetworkService _networkService;
    private readonly SelectionManager _selectionManager;
    
    /// <summary>
    ///     Search for the design we'd like to send
    /// </summary>
    public string SearchTerm = string.Empty;
    
    /// <summary>
    ///     The currently selected Guid of the Design to send
    /// </summary>
    public Guid SelectedDesignId = Guid.Empty;
    
    /// <summary>
    ///     Cached list of designs
    /// </summary>
    private List<FolderNode<Design>>? _sorted;
    
    /// <summary>
    ///     Filtered cached list of designs
    /// </summary>
    private List<FolderNode<Design>>? _filtered;
    
    /// <summary>
    ///     The designs to display in the Ui
    /// </summary>
    public List<FolderNode<Design>>? Designs => SearchTerm == string.Empty ? _sorted : _filtered;
    
    public bool ShouldApplyCustomization = true;
    public bool ShouldApplyEquipment = true;

    /// <summary>
    ///     <inheritdoc cref="TransformationViewUiController"/>
    /// </summary>
    public TransformationViewUiController(CommandLockoutService commandLockoutService, GlamourerService glamourer, NetworkService networkService, SelectionManager selectionManager)
    {
        _commandLockoutService = commandLockoutService;
        _glamourerService = glamourer;
        _networkService = networkService;
        _selectionManager = selectionManager;

        _glamourerService.IpcReady += OnIpcReady;
        if (_glamourerService.ApiAvailable)
            _ = RefreshGlamourerDesigns();
    }
    
    /// <summary>
    ///     Filters the sorted design list by search term
    /// </summary>
    public void FilterDesignsBySearchTerm()
    {
        _filtered = _sorted is not null 
            ? FilterFolderNodes(_sorted, SearchTerm).ToList() 
            : null;
    }
    
    /// <summary>
    ///     Recursive method to filter nodes based on both folders and content names
    /// </summary>
    private List<FolderNode<Design>> FilterFolderNodes(IEnumerable<FolderNode<Design>> nodes, string searchTerms)
    {
        // Reset the selected so possibly unselected designs aren't stored
        SelectedDesignId = Guid.Empty;
        
        // Iterate to determine what stays and what goes
        var results = new List<FolderNode<Design>>();
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
            results.Add(new FolderNode<Design>(node.Name, node.Content, children));
        }
        
        return results;
    }
    
    public async Task RefreshGlamourerDesigns()
    {
        SelectedDesignId = Guid.Empty;

        _sorted = await _glamourerService.GetDesignList2().ConfigureAwait(false) is { } unsorted
            ? unsorted.Children.Values.ToList()
            : null;
    }
    
    public bool MissingPermissionsForATarget()
    {
        foreach (var friend in _selectionManager.Selected)
        {
            if (friend.PermissionsGrantedByFriend is null)
                continue;
            
            if (ShouldApplyCustomization)
                if ((friend.PermissionsGrantedByFriend.Primary & PrimaryPermissions.GlamourerCustomization) is not PrimaryPermissions.GlamourerCustomization)
                    return true;
            
            if (ShouldApplyEquipment)
                if ((friend.PermissionsGrantedByFriend.Primary & PrimaryPermissions.GlamourerEquipment) is not PrimaryPermissions.GlamourerEquipment)
                    return true;
        }
        
        return false;
    }

    public async Task SendDesign()
    {
        if (SelectedDesignId == Guid.Empty)
            return;
            
        if (await _glamourerService.GetDesignAsync(SelectedDesignId).ConfigureAwait(false) is not { } design)
            return;
            
        _commandLockoutService.Lock();

        var flags = GlamourerApplyFlags.Once;
        if (ShouldApplyCustomization)
            flags |= GlamourerApplyFlags.Customization;
        if (ShouldApplyEquipment)
            flags |= GlamourerApplyFlags.Equipment;

        // Don't send one with nothing
        if (flags is GlamourerApplyFlags.Once)
            return;
            
        var request = new TransformRequest(_selectionManager.GetSelectedFriendCodes(), design, flags, null);
        var response = await _networkService.InvokeAsync<ActionResponse>(HubMethod.Transform, request).ConfigureAwait(false);
            
        ActionResponseParser.Parse("Transformation", response);
    }
    
    private void OnIpcReady(object? sender, EventArgs e)
    {
        _ = RefreshGlamourerDesigns();
    }

    public void Dispose()
    {
        _glamourerService.IpcReady -= OnIpcReady;
        GC.SuppressFinalize(this);
    }
}