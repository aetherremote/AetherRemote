using System;
using System.Collections.Generic;
using System.Linq;
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
    
    // TODO: More commenting
    
    public string SearchTerm = string.Empty;
    
    private List<Folder<Design>> _designs = [];
    public List<Folder<Design>> FilteredDesigns => SearchTerm == string.Empty 
        ? _designs.ToList()
        : _designs.Select(folder => new Folder<Design>(folder.Path, folder.Content.Where(design => design.Name.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase)).ToList())).ToList();
    
    public Guid SelectedDesignId = Guid.Empty;
    public bool ShouldApplyCustomization = true;
    public bool ShouldApplyEquipment = true;

    public TransformationViewUiController(CommandLockoutService commandLockoutService, GlamourerService glamourer, NetworkService networkService, SelectionManager selectionManager)
    {
        _commandLockoutService = commandLockoutService;
        _glamourerService = glamourer;
        _networkService = networkService;
        _selectionManager = selectionManager;

        _glamourerService.IpcReady += OnIpcReady;
        if (_glamourerService.ApiAvailable)
            RefreshDesigns();
    }
    
    public async void RefreshDesigns()
    {
        try
        {
            SelectedDesignId = Guid.Empty;

            _designs = await _glamourerService.GetDesignList().ConfigureAwait(false);
        }
        catch (Exception)
        {
            // Ignored
        }
    }
    
    public bool MissingPermissionsForATarget()
    {
        foreach (var friend in _selectionManager.Selected)
        {
            if (friend.PermissionsGrantedByFriend is null)
                continue;
            
            if (ShouldApplyCustomization)
                if ((friend.PermissionsGrantedByFriend.Primary & PrimaryPermissions2.GlamourerCustomization) is not PrimaryPermissions2.GlamourerCustomization)
                    return true;
            
            if (ShouldApplyEquipment)
                if ((friend.PermissionsGrantedByFriend.Primary & PrimaryPermissions2.GlamourerEquipment) is not PrimaryPermissions2.GlamourerEquipment)
                    return true;
        }
        
        return false;
    }

    public async void SendDesign()
    {
        try
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
        catch (Exception)
        {
            // Ignored
        }
    }
    
    private void OnIpcReady(object? sender, EventArgs e)
    {
        RefreshDesigns();
    }

    public void Dispose()
    {
        _glamourerService.IpcReady -= OnIpcReady;
        GC.SuppressFinalize(this);
    }
}