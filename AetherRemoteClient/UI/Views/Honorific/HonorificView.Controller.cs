using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AetherRemoteClient.Domain.Honorific;
using AetherRemoteClient.Services;
using AetherRemoteClient.Utils;
using AetherRemoteClient.Utils.Extensions;
using AetherRemoteCommon.Domain.Enums.Permissions;
using AetherRemoteCommon.Domain.Network;
using AetherRemoteCommon.Domain.Network.Honorific;

namespace AetherRemoteClient.UI.Views.Honorific;

public partial class HonorificView
{
    private string _searchTerm = string.Empty;

    private HonorificCustomTitle? _selectedTitle;
    
    private Dictionary<string, List<HonorificCustomTitle>> _titles = [];

    private Dictionary<string, List<HonorificCustomTitle>> FilteredTitles => _searchTerm == string.Empty
        ? _titles.ToDictionary()
        : FilterTitles();

    private async Task RefreshTitles()
    {
        _selectedTitle = null;
            
        var titles = await HonorificService.GetCharacterTitleList().ConfigureAwait(false);

        var final = new Dictionary<string, List<HonorificCustomTitle>>();
        foreach (var (worldId, dictionary) in titles)
        {
            if (_worldService.TryGetWorldById(worldId > ushort.MaxValue ? ushort.MaxValue : (ushort)worldId) is not { } worldName)
                continue;

            foreach (var (character, configuration) in dictionary)
                final[$"{character} - {worldName}"] = configuration;
        }

        _titles = final;
    }

    private bool MissingPermissionsForATarget()
    {
        foreach (var friend in _selectionManager.Selected)
        {
            if (friend.PermissionsGrantedByFriend is null)
                continue;
            
            if ((friend.PermissionsGrantedByFriend.Primary & PrimaryPermissions.Honorific) is not PrimaryPermissions.Honorific)
                return true;
        }
        
        return false;
    }

    private async Task SendHonorific()
    {
        if (_selectedTitle == null)
            return;
            
        _commandLockoutService.Lock();
            
        var request = new HonorificRequest(_selectionManager.GetSelectedFriendCodes(), _selectedTitle.ToHonorificDto());
        var response = await _networkService.InvokeAsync<ActionResponse>(HubMethod.Honorific, request).ConfigureAwait(false);
        ActionResponseParser.Parse("Honorific", response, _notesService.Notes);
    }
    
    private void OnIpcReady(object? sender, EventArgs e)
    {
        _ = RefreshTitles().ConfigureAwait(false);
    }
    
    /// <summary>
    ///     Function to filter out the original dictionary to retrieve only the 
    /// </summary>
    /// <returns></returns>
    private Dictionary<string, List<HonorificCustomTitle>> FilterTitles()
    {
        var result = new Dictionary<string, List<HonorificCustomTitle>>();
        foreach (var (character, titles) in _titles)
        {
            var list = new List<HonorificCustomTitle>();
            foreach (var title in titles)
                if (title.Title.Contains(_searchTerm, StringComparison.OrdinalIgnoreCase))
                    list.Add(title);
            
            if (list.Count > 0)
                result.Add(character, list);
        }

        return result;
    }
}