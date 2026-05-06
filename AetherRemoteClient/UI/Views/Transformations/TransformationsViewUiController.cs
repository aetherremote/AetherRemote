using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AetherRemoteClient.Domain;
using AetherRemoteClient.Domain.Enums;
using AetherRemoteClient.Domain.Glamourer;
using AetherRemoteClient.Managers;
using AetherRemoteClient.Services;
using AetherRemoteClient.Utils;
using AetherRemoteCommon.Domain.Enums;
using AetherRemoteCommon.Domain.Enums.Permissions;
using AetherRemoteCommon.Domain.Network;
using AetherRemoteCommon.Domain.Network.BodySwap;

namespace AetherRemoteClient.UI.Views.Transformations;

/// <summary>
///     Various controllers for transformations
/// </summary>
public class TransformationsViewUiController : IDisposable
{
    // Injected
    private readonly CommandLockoutService _commandLockoutService;
    private readonly GlamourerService _glamourerService;
    private readonly NetworkService _networkService;
    private readonly CharacterTransformationManager _characterTransformationManager;
    private readonly NetworkCommandManager _networkCommandManager;
    private readonly SelectionManager _selectionManager;
    private readonly StatusManager _statusManager;
    
    /// <summary>
    ///     What mode the Ui will display, and how network events will be sent
    /// </summary>
    public TransformationMode Mode = TransformationMode.Transform;
    
    /// <summary>
    ///     The name of the design we are searching for
    /// </summary>
    public string DesignSearchTerm = string.Empty;
    
    /// <summary>
    ///     Selected design guid
    /// </summary>
    public Guid DesignSelectedId = Guid.Empty;
    
    /// <summary>
    ///     Should swap glamourer customizations (enabled by default)
    /// </summary>
    public bool SwapGlamourerCustomization = true;
    
    /// <summary>
    ///     Should swap glamourer equipment (enabled by default)
    /// </summary>
    public bool SwapGlamourerEquipment = true;
    
    /// <summary>
    ///     Should swap penumbra mods
    /// </summary>
    public bool SwapPenumbraMods;
    
    /// <summary>
    ///     Should swap moodles
    /// </summary>
    public bool SwapMoodles;
    
    /// <summary>
    ///     Should swap customize plus
    /// </summary>
    public bool SwapCustomizePlus;
    
    /// <summary>
    ///     Should swap honorific
    /// </summary>
    public bool SwapHonorific;
    
    /// <summary>
    ///     Finalized designs
    /// </summary>
    public List<FolderNode<Design>>? Designs => DesignSearchTerm == string.Empty ? _sorted : _filtered;

    /// <summary>
    ///     <inheritdoc cref="TransformationsViewUiController"/>
    /// </summary>
    public TransformationsViewUiController(
        CommandLockoutService commandLockoutService, 
        GlamourerService glamourerService, 
        NetworkService networkService,
        CharacterTransformationManager characterTransformationManager,
        NetworkCommandManager networkCommandManager, 
        SelectionManager selectionManager,
        StatusManager statusManager)
    {
        _commandLockoutService = commandLockoutService;
        _glamourerService = glamourerService;
        _networkService = networkService;
        _characterTransformationManager = characterTransformationManager;
        _networkCommandManager = networkCommandManager;
        _selectionManager = selectionManager;
        _statusManager = statusManager;
        
        _glamourerService.IpcReady += OnIpcReady;
        if (_glamourerService.ApiAvailable)
            _ = RefreshGlamourerDesigns();
    }
    
    // Lists for both a cached filtered, and sorted variants of the folder structure
    private List<FolderNode<Design>>? _sorted;
    private List<FolderNode<Design>>? _filtered;
    
    /// <summary>
    ///     Filters the sorted design list by search term
    /// </summary>
    public void FilterDesignsBySearchTerm() => _filtered = _sorted is not null ? FilterFolderNodes(_sorted, DesignSearchTerm).ToList() : null;
    
    /// <summary>
    ///     Refreshes the list of glamourer designs and populates the Ui elements with the fresh data
    /// </summary>
    public async Task RefreshGlamourerDesigns()
    {
        DesignSelectedId = Guid.Empty;

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
    public bool MissingPermissionsForATarget()
    {
        foreach (var friend in _selectionManager.Selected)
        {
            if (friend.PermissionsGrantedByFriend is null)
                continue;
            
            if (SwapGlamourerCustomization)
                if ((friend.PermissionsGrantedByFriend.Primary & PrimaryPermissions.Glamourer) is not PrimaryPermissions.Glamourer)
                    return true;
            
            if (SwapGlamourerEquipment)
                if ((friend.PermissionsGrantedByFriend.Primary & PrimaryPermissions.Glamourer) is not PrimaryPermissions.Glamourer)
                    return true;
            
            if (Mode == TransformationMode.Transform)
                continue; // Transform only deals with glamourer, so we can skip
            
            if (SwapPenumbraMods)
                if ((friend.PermissionsGrantedByFriend.Primary & PrimaryPermissions.Mods) is not PrimaryPermissions.Mods)
                    return true;
            
            if (SwapCustomizePlus)
                if ((friend.PermissionsGrantedByFriend.Primary & PrimaryPermissions.CustomizePlus) is not PrimaryPermissions.CustomizePlus)
                    return true;
            
            if (SwapHonorific)
                if ((friend.PermissionsGrantedByFriend.Primary & PrimaryPermissions.Honorific) is not PrimaryPermissions.Honorific)
                    return true;
            
            if (SwapMoodles)
                if ((friend.PermissionsGrantedByFriend.Primary & PrimaryPermissions.Moodles) is not PrimaryPermissions.Moodles)
                    return true;
        }
        
        return false;
    }

    /// <summary>
    ///     Sends the command to the server based on what mode is selected
    /// </summary>
    public async Task Send()
    {
        switch (Mode)
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
        if (DesignSelectedId == Guid.Empty)
            return;
        
        // Get the glamourer design
        if (await _glamourerService.GetDesignAsync(DesignSelectedId).ConfigureAwait(false) is not { } design)
            return;
        
        // Send
        await _networkCommandManager.SendTransformation(_selectionManager.GetSelectedFriendCodes(), design, GlamourerApplyFlags.All).ConfigureAwait(false);
    }
    
    private async Task SendBodySwap()
    {
        // Basic validation checks
        if (Plugin.CharacterConfiguration is not { } character) return;
        
        // Build the attributes
        var attributes = CharacterAttributes.None;
        if (SwapGlamourerCustomization) attributes |= CharacterAttributes.GlamourerCustomization;
        if (SwapGlamourerEquipment) attributes |= CharacterAttributes.GlamourerEquipment;
        if (SwapPenumbraMods) attributes |= CharacterAttributes.PenumbraMods;
        if (SwapMoodles) attributes |= CharacterAttributes.Moodles;
        if (SwapCustomizePlus) attributes |= CharacterAttributes.CustomizePlus;
        if (SwapHonorific) attributes |= CharacterAttributes.Honorific;
        
        // Notification to help convey intent
        NotificationHelper.Info("Beginning Body Swap...", "You may need to wait up to 10 seconds for changes to take effect");
        
        // Body swapping has a custom workflow for the time being
        _commandLockoutService.Lock();
        
        // Request the server
        var request = new BodySwapRequest(_selectionManager.GetSelectedFriendCodes(), character.Name, character.World, attributes, null);
        var response = await _networkService.InvokeAsync<BodySwapResponse>(HubMethod.BodySwap, request);
        if (response.Result is not ActionResponseEc.Success)
        {
            ActionResponseParser.Parse("Body Swap", response);
            return;
        }
        
        // If the character we'd be body swapping into was null...
        if (response.CharacterName is null || response.CharacterWorld is null)
        {
            // ...but we expected to get back a result by submitting our name in the body swap request...
            if (request.SenderCharacterName is not null)
            {
                // ...exit and log the error
                return;
            }
        }
        else
        {
            // Otherwise just body swap into them
            await _characterTransformationManager.ApplyFullScaleTransformation(response.CharacterName, response.CharacterWorld, request.SwapAttributes);
         
            // TODO: This is just a copy from the NetworkHandler
            if ((attributes & CharacterAttributes.PenumbraMods) is CharacterAttributes.PenumbraMods)
                _statusManager.SetGlamourerPenumbra(Friend.Self);
        
            if ((attributes & CharacterAttributes.CustomizePlus) is CharacterAttributes.CustomizePlus)
                _statusManager.SetCustomizePlus(Friend.Self);
        
            if ((attributes & CharacterAttributes.Honorific) is CharacterAttributes.Honorific)
                _statusManager.SetHonorific(Friend.Self);
        }
            
        // Process the results
        ActionResponseParser.Parse("Body Swap", response);
    }
    
    private async Task SendTwinning()
    {
        // Basic validation checks
        if (Plugin.CharacterConfiguration is not { } character) return;
        
        // Build the attributes
        var attributes = CharacterAttributes.None;
        if (SwapGlamourerCustomization) attributes |= CharacterAttributes.GlamourerCustomization;
        if (SwapGlamourerEquipment) attributes |= CharacterAttributes.GlamourerEquipment;
        if (SwapPenumbraMods) attributes |= CharacterAttributes.PenumbraMods;
        if (SwapMoodles) attributes |= CharacterAttributes.Moodles;
        if (SwapCustomizePlus) attributes |= CharacterAttributes.CustomizePlus;
        if (SwapHonorific) attributes |= CharacterAttributes.Honorific;
        
        // Notification to help convey intent
        NotificationHelper.Info("Beginning Twinning...", "You may need to wait up to 10 seconds for changes to take effect");
        
        // Send
        await _networkCommandManager.SendTwinning(_selectionManager.GetSelectedFriendCodes(), character.Name, character.World, attributes).ConfigureAwait(false);
    }
    
    /// <summary>
    ///     Recursive method to filter nodes based on both folders and content names
    /// </summary>
    private List<FolderNode<Design>> FilterFolderNodes(IEnumerable<FolderNode<Design>> nodes, string searchTerms)
    {
        // Reset the selected so possibly unselected designs aren't stored
        DesignSelectedId = Guid.Empty;
        
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
        _ = RefreshGlamourerDesigns();
    }

    public void Dispose()
    {
        _glamourerService.IpcReady -= OnIpcReady;
        GC.SuppressFinalize(this);
    }
}