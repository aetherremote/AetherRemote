using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
    private readonly CommandLockoutService _commandLockoutService;
    private readonly CustomizePlusService _customizePlusService;
    private readonly NetworkService _networkService;
    private readonly SelectionManager _selectionManager;
    
    /// <summary>
    ///     Customize+ data to send
    /// </summary>
    public string SearchTerm = string.Empty;
    
    /// <summary>
    ///     The currently selected Guid of the Profile to send
    /// </summary>
    public Guid SelectedProfileId = Guid.Empty;

    private List<Folder<Profile>> _profiles = [];
    public List<Folder<Profile>> FilteredProfiles => SearchTerm == string.Empty
        ? _profiles.ToList()
        : _profiles.Select(folder => new Folder<Profile>(folder.Path, folder.Content.Where(design => design.Name.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase)).ToList())).ToList();
    
    /// <summary>
    ///     <inheritdoc cref="CustomizePlusViewUiController"/>
    /// </summary>
    public CustomizePlusViewUiController(CommandLockoutService commandLockoutService, CustomizePlusService customizePlusService, NetworkService networkService, SelectionManager selectionManager)
    {
        _commandLockoutService = commandLockoutService;
        _customizePlusService = customizePlusService;
        _networkService = networkService;
        _selectionManager = selectionManager;

        _customizePlusService.IpcReady += OnIpcReady;
        if (_customizePlusService.ApiAvailable)
            RefreshCustomizeProfiles();
    }

    public async void RefreshCustomizeProfiles()
    {
        try
        {
            SelectedProfileId = Guid.Empty;
            
            _profiles = await _customizePlusService.GetProfiles().ConfigureAwait(false);
        }
        catch (Exception)
        {
            // Ignored
        }
    }
    
    /// <summary>
    ///     Calculates the friends who you lack correct permissions to send to
    /// </summary>
    /// <returns></returns>
    public bool MissingPermissionsForATarget()
    {
        foreach (var friend in _selectionManager.Selected)
        {
            if (friend.PermissionsGrantedByFriend is null)
                continue;
            
            if ((friend.PermissionsGrantedByFriend.Primary & PrimaryPermissions2.CustomizePlus) is not PrimaryPermissions2.CustomizePlus)
                return true;
        }

        return false;
    }
    
    public async void SendCustomizeProfile()
    {
        try
        {
            if (SelectedProfileId == Guid.Empty)
                return;

            _commandLockoutService.Lock();
        
            if (await _customizePlusService.GetProfile(SelectedProfileId).ConfigureAwait(false) is not { } profile)
                return;

            var bytes = Encoding.UTF8.GetBytes(profile);
            var request = new CustomizeRequest(_selectionManager.GetSelectedFriendCodes(), bytes);
            var response = await _networkService.InvokeAsync<ActionResponse>(HubMethod.CustomizePlus, request).ConfigureAwait(false);

            ActionResponseParser.Parse("Customize+", response);
        }
        catch (Exception)
        {
            // Ignored
        }
    }
    
    private void OnIpcReady(object? sender, EventArgs e)
    {
        RefreshCustomizeProfiles();
    }

    public void Dispose()
    {
        _customizePlusService.IpcReady -= OnIpcReady;
        GC.SuppressFinalize(this);
    }
}