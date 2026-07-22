using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AetherRemoteClient.Domain;
using AetherRemoteClient.Domain.Enums;
using AetherRemoteClient.Domain.Glamourer;
using AetherRemoteClient.Utils;
using AetherRemoteCommon.Domain.Enums;
using AetherRemoteCommon.Domain.Enums.Permissions;
using AetherRemoteCommon.Domain.Network;
using AetherRemoteCommon.Network.Domain;
using AetherRemoteCommon.Network.Domain.Payloads;
using AetherRemoteCommon.Network.Enums;

namespace AetherRemoteClient.UI.Views.Transformations;

/// <summary>
///     Various controllers for transformations
/// </summary>
public partial class TransformationsView
{
    /// <summary>
    ///     What mode the Ui will display, and how network events will be sent
    /// </summary>
    private TransformationMode _mode = TransformationMode.Transform;
    
    /// <summary>
    ///     The name of the design we are searching for
    /// </summary>
    private string _designSearchTerm = string.Empty;
    
    /// <summary>
    ///     Selected design guid
    /// </summary>
    private Guid _designSelectedId = Guid.Empty;
    
    /// <summary>
    ///     Should swap glamourer customizations (enabled by default)
    /// </summary>
    private bool _swapGlamourerCustomization = true;
    
    /// <summary>
    ///     Should swap glamourer equipment (enabled by default)
    /// </summary>
    private bool _swapGlamourerEquipment = true;
    
    /// <summary>
    ///     Should swap penumbra mods
    /// </summary>
    private bool _swapPenumbraMods;
    
    /// <summary>
    ///     Should swap moodles
    /// </summary>
    private bool _swapMoodles;
    
    /// <summary>
    ///     Should swap customize plus
    /// </summary>
    private bool _swapCustomizePlus;
    
    /// <summary>
    ///     Should swap honorific
    /// </summary>
    private bool _swapHonorific;
    
    /// <summary>
    ///     Finalized designs
    /// </summary>
    private List<FolderNode<Design>>? Designs => _designSearchTerm == string.Empty ? _sorted : _filtered;
    
    // Lists for both a cached filtered, and sorted variants of the folder structure
    private List<FolderNode<Design>>? _sorted;
    private List<FolderNode<Design>>? _filtered;
    
    /// <summary>
    ///     Filters the sorted design list by search term
    /// </summary>
    private void FilterDesignsBySearchTerm() => _filtered = _sorted is not null ? FilterFolderNodes(_sorted, _designSearchTerm).ToList() : null;
    
    /// <summary>
    ///     Refreshes the list of glamourer designs and populates the Ui elements with the fresh data
    /// </summary>
    private async Task RefreshGlamourerDesigns()
    {
        _designSelectedId = Guid.Empty;

        if (await _glamourerService.GetDesignList().ConfigureAwait(false) is not { } designs)
            return;
        
        var root = new FolderNode<Design>("Root", null);
        foreach (var design in designs)
        {
            var parts = design.Path.Split('/', StringSplitOptions.RemoveEmptyEntries);
            var current = root;

            for (var i = 0; i < parts.Length; i++)
            {
                var part = parts[i];
                if (current.Children.TryGetValue(part, out var node) is false)
                {
                    node = new FolderNode<Design>(part, i == parts.Length - 1 ? design : null);
                    current.Children[part] = node;
                }

                current = node;
            }
        }

        // The dictionary provided by glamourer is not sorted
        SortTree(root);
        
        // Assignment
        _sorted = root.Children.Values.ToList();
    }

    /// <summary>
    ///     Tests if the batch of currently selected targets have all the permissions required for what you're trying to do
    /// </summary>
    /// <returns></returns>
    private bool MissingPermissionsForATarget()
    {
        foreach (var friend in _selectionManager.Selected)
        {
            if (friend.PermissionsGrantedByFriend is null)
                continue;
            
            if (_swapGlamourerCustomization)
                if ((friend.PermissionsGrantedByFriend.Primary & PrimaryPermissions.Glamourer) is not PrimaryPermissions.Glamourer)
                    return true;
            
            if (_swapGlamourerEquipment)
                if ((friend.PermissionsGrantedByFriend.Primary & PrimaryPermissions.Glamourer) is not PrimaryPermissions.Glamourer)
                    return true;
            
            if (_mode == TransformationMode.Transform)
                continue; // Transform only deals with glamourer, so we can skip
            
            if (_swapPenumbraMods)
                if ((friend.PermissionsGrantedByFriend.Primary & PrimaryPermissions.Mods) is not PrimaryPermissions.Mods)
                    return true;
            
            if (_swapCustomizePlus)
                if ((friend.PermissionsGrantedByFriend.Primary & PrimaryPermissions.CustomizePlus) is not PrimaryPermissions.CustomizePlus)
                    return true;
            
            if (_swapHonorific)
                if ((friend.PermissionsGrantedByFriend.Primary & PrimaryPermissions.Honorific) is not PrimaryPermissions.Honorific)
                    return true;
            
            if (_swapMoodles)
                if ((friend.PermissionsGrantedByFriend.Primary & PrimaryPermissions.Moodles) is not PrimaryPermissions.Moodles)
                    return true;
        }
        
        return false;
    }

    /// <summary>
    ///     Sends the command to the server based on what mode is selected
    /// </summary>
    private async Task Send()
    {
        switch (_mode)
        {
            // We don't want to operate on this yet
            case TransformationMode.Mimicry:
                return;
            
            case TransformationMode.Transform:
                await SendTransform().ConfigureAwait(false);
                break;
            
            case TransformationMode.BodySwap:
                await SendBodySwap().ConfigureAwait(false);
                break;
            
            case TransformationMode.Twinning:
                await SendTwinning().ConfigureAwait(false);
                break;
            
            default:
                return;
        }
    }

    private async Task SendTransform()
    {
        // Basic validation checks
        if (_designSelectedId == Guid.Empty)
            return;
        
        // Get the glamourer design
        if (await _glamourerService.GetDesignAsync(_designSelectedId).ConfigureAwait(false) is not { } design)
            return;
        
        var targets = _selectionManager.GetSelectedFriendCodes();
        if (targets.Count is 0)
            return;
        
        _commandLockoutService.Lock();
        var payload = new TransformationPayload(design, GlamourerApplyFlags.All, null);
        await _networkRequestManager.Send<TransformationPayload, NoPayload>(targets, HubMethod.Transform, payload).ConfigureAwait(false);
    }
    
    private async Task SendBodySwap()
    {
        // Build the attributes
        var attributes = CharacterAttributes.None;
        if (_swapGlamourerCustomization) attributes |= CharacterAttributes.GlamourerCustomization;
        if (_swapGlamourerEquipment) attributes |= CharacterAttributes.GlamourerEquipment;
        if (_swapPenumbraMods) attributes |= CharacterAttributes.PenumbraMods;
        if (_swapMoodles) attributes |= CharacterAttributes.Moodles;
        if (_swapCustomizePlus) attributes |= CharacterAttributes.CustomizePlus;
        if (_swapHonorific) attributes |= CharacterAttributes.Honorific;
        
        // Notification to help convey intent
        NotificationHelper.Info("Beginning Body Swap...", "You may need to wait up to 10 seconds for changes to take effect");
        
        var targets = _selectionManager.GetSelectedFriendCodes();
        if (targets.Count is 0)
            return;
        
        // TODO: Always including self for now, I will decouple the transformation operations
        //          This will also need to solve A, B, and C
        
        _commandLockoutService.Lock();
        var payload = new BodySwapPayload(attributes, true, null);
        var response = await _networkRequestManager.Send<BodySwapPayload, BodySwapResponse>(targets, HubMethod.Transform, payload).ConfigureAwait(false);

        if (response.Status is not ResponseStatus.Success)
            return;

        // TODO: A
        if (response.Payload is not { } bodySwapResponse)
            return;

        // TODO: B
        await _characterTransformationManager.ApplyFullScaleTransformation(bodySwapResponse.CharacterName, bodySwapResponse.CharacterWorld, attributes).ConfigureAwait(false);
        
        // TODO: C
        if ((attributes & CharacterAttributes.PenumbraMods) is CharacterAttributes.PenumbraMods)
            _statusService.SetGlamourerPenumbra(Friend.Self);
        
        if ((attributes & CharacterAttributes.CustomizePlus) is CharacterAttributes.CustomizePlus)
            _statusService.SetCustomizePlus(Friend.Self);
        
        if ((attributes & CharacterAttributes.Honorific) is CharacterAttributes.Honorific)
            _statusService.SetHonorific(Friend.Self);
    }
    
    private async Task SendTwinning()
    {
        // Basic validation checks
        if (_activeSessionService.CharacterName is not { } name ||
            _activeSessionService.CharacterWorld is not { } world)
            return;
        
        // Build the attributes
        var attributes = CharacterAttributes.None;
        if (_swapGlamourerCustomization) attributes |= CharacterAttributes.GlamourerCustomization;
        if (_swapGlamourerEquipment) attributes |= CharacterAttributes.GlamourerEquipment;
        if (_swapPenumbraMods) attributes |= CharacterAttributes.PenumbraMods;
        if (_swapMoodles) attributes |= CharacterAttributes.Moodles;
        if (_swapCustomizePlus) attributes |= CharacterAttributes.CustomizePlus;
        if (_swapHonorific) attributes |= CharacterAttributes.Honorific;
        
        // Notification to help convey intent
        NotificationHelper.Info("Beginning Twinning...", "You may need to wait up to 10 seconds for changes to take effect");
        
        var targets = _selectionManager.GetSelectedFriendCodes();
        if (targets.Count is 0)
            return;
        
        _commandLockoutService.Lock();
        var payload = new TwinningPayload(name, world, attributes, null);
        await _networkRequestManager.Send<TwinningPayload, NoPayload>(targets, HubMethod.Twinning, payload).ConfigureAwait(false);
    }
    
    /// <summary>
    ///     Recursive method to filter nodes based on both folders and content names
    /// </summary>
    private List<FolderNode<Design>> FilterFolderNodes(IEnumerable<FolderNode<Design>> nodes, string searchTerms)
    {
        // Reset the selected so possibly unselected designs aren't stored
        _designSelectedId = Guid.Empty;
        
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
    
    /// <summary>
    ///     The dictionary returned by glamourer is not sorted, so we will recursively go through and sort the children
    /// </summary>
    private static void SortTree<T>(FolderNode<T> root)
    {
        // Copy all the children from this node and sort them by folder, then name
        var sorted = root.Children.Values
            .OrderByDescending(node => node.IsFolder)
            .ThenBy(node => node.Name, StringComparer.OrdinalIgnoreCase)
            .ToList();
        
        // Clear all the children with the values sorted and copied
        root.Children.Clear();

        // Reintroduce because dictionaries preserve insertion order
        foreach (var node in sorted)
            root.Children[node.Name] = node;
        
        // Recursively sort the remaining children
        foreach (var child in root.Children.Values)
            SortTree(child);
    }
    
    /// <summary>
    ///     Light wrapper async wrapper for when the event is fired
    /// </summary>
    private void OnIpcReady(object? sender, EventArgs e)
    {
        _ = RefreshGlamourerDesigns().ConfigureAwait(false);
    }
}