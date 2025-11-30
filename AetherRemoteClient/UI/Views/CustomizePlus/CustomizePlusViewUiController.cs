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
using AetherRemoteCommon.Domain.Network;
using AetherRemoteCommon.Domain.Network.Customize;

namespace AetherRemoteClient.UI.Views.CustomizePlus;

public class CustomizePlusViewUiController(CommandLockoutService commandLockoutService, CustomizePlusService customizePlusService, NetworkService networkService, SelectionManager selectionManager)
{
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

    public async void RefreshCustomizeProfiles()
    {
        try
        {
            SelectedProfileId = Guid.Empty;
            
            _profiles = await customizePlusService.GetProfiles().ConfigureAwait(false);
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
    public List<string> GetFriendsLackingPermissions()
    {
        return [];
    }
    
    public async void SendCustomizeProfile()
    {
        try
        {
            if (SelectedProfileId == Guid.Empty)
                return;

            commandLockoutService.Lock();
        
            if (await customizePlusService.GetProfile(SelectedProfileId).ConfigureAwait(false) is not { } profile)
                return;

            var bytes = Encoding.UTF8.GetBytes(profile);
            var string64 = Convert.ToBase64String(bytes);

            var request = new CustomizeRequest(selectionManager.GetSelectedFriendCodes(), string64);
            var response = await networkService.InvokeAsync<ActionResponse>(HubMethod.CustomizePlus, request).ConfigureAwait(false);

            ActionResponseParser.Parse("Customize+", response);
        }
        catch (Exception)
        {
            // Ignored
        }
    }
}