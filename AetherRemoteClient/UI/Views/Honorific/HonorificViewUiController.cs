using System;
using System.Collections.Generic;
using System.Linq;
using AetherRemoteClient.Dependencies.Honorific.Services;
using AetherRemoteClient.Managers;
using AetherRemoteClient.Services;
using AetherRemoteClient.Utils;
using AetherRemoteCommon.Dependencies.Honorific.Domain;
using AetherRemoteCommon.Domain.Enums.Permissions;
using AetherRemoteCommon.Domain.Network;
using AetherRemoteCommon.Domain.Network.Honorific;

namespace AetherRemoteClient.UI.Views.Honorific;

public class HonorificViewUiController : IDisposable
{
    // Injected
    private readonly CommandLockoutService _commandLockout;
    private readonly HonorificService _honorific;
    private readonly NetworkService _network;
    private readonly WorldService _world;
    private readonly SelectionManager _selection;
    
    public string SearchTerm = string.Empty;
    
    public HonorificInfo? SelectedTitle;
    
    private Dictionary<string, List<HonorificInfo>> _titles = [];
    public Dictionary<string, List<HonorificInfo>> FilteredTitles => SearchTerm == string.Empty
        ? _titles.ToDictionary()
        : FilterTitles();
    
    public HonorificViewUiController(CommandLockoutService commandLockout, HonorificService honorific, NetworkService network, WorldService world, SelectionManager selection)
    {
        _commandLockout = commandLockout;
        _honorific = honorific;
        _network = network;
        _world = world;
        _selection = selection;

        _honorific.IpcReady += OnIpcReady;
        if (_honorific.ApiAvailable)
            RefreshTitles();
    }

    public async void RefreshTitles()
    {
        try
        {
            SelectedTitle = null;
            
            var titles = await HonorificService.GetCharacterTitleList().ConfigureAwait(false);

            var final = new Dictionary<string, List<HonorificInfo>>();
            foreach (var (worldId, dictionary) in titles)
            {
                if (_world.TryGetWorldById(worldId > ushort.MaxValue ? ushort.MaxValue : (ushort)worldId) is not { } worldName)
                    continue;

                foreach (var (character, configuration) in dictionary)
                    final[$"{character} - {worldName}"] = configuration;
            }

            _titles = final;
        }
        catch (Exception)
        {
            // Ignored
        }
    }
    
    public bool MissingPermissionsForATarget()
    {
        foreach (var friend in _selection.Selected)
            if ((friend.PermissionsGrantedByFriend.Primary & PrimaryPermissions2.Honorific) is not PrimaryPermissions2.Honorific)
                return true;
        
        return false;
    }

    public async void SendHonorific()
    {
        try
        {
            if (SelectedTitle == null)
                return;
            
            _commandLockout.Lock();
            
            var request = new HonorificRequest(_selection.GetSelectedFriendCodes(), SelectedTitle);
            var response = await _network.InvokeAsync<ActionResponse>(HubMethod.Honorific, request).ConfigureAwait(false);
            ActionResponseParser.Parse("Honorific", response);
        }
        catch (Exception)
        {
            // Ignore
        }
    }
    
    private void OnIpcReady(object? sender, EventArgs e)
    {
        RefreshTitles();
    }

    public void Dispose()
    {
        _honorific.IpcReady -= OnIpcReady;
        GC.SuppressFinalize(this);
    }
    
    /// <summary>
    ///     Function to filter out the original dictionary to retrieve only the 
    /// </summary>
    /// <returns></returns>
    private Dictionary<string, List<HonorificInfo>> FilterTitles()
    {
        var result = new Dictionary<string, List<HonorificInfo>>();
        foreach (var (character, titles) in _titles)
        {
            var list = new List<HonorificInfo>();
            foreach (var title in titles)
                if (title.Title?.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase) ?? false)
                    list.Add(title);
            
            if (list.Count > 0)
                result.Add(character, list);
        }

        return result;
    }
}