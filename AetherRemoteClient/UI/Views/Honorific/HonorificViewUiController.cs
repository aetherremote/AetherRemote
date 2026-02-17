using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AetherRemoteClient.Dependencies.Honorific.Domain;
using AetherRemoteClient.Dependencies.Honorific.Services;
using AetherRemoteClient.Managers;
using AetherRemoteClient.Services;
using AetherRemoteClient.Utils;
using AetherRemoteClient.Utils.Extensions;
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
    
    public HonorificCustomTitle? SelectedTitle;
    
    private Dictionary<string, List<HonorificCustomTitle>> _titles = [];
    public Dictionary<string, List<HonorificCustomTitle>> FilteredTitles => SearchTerm == string.Empty
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
            _ = RefreshTitles().ConfigureAwait(false);
    }

    public async Task RefreshTitles()
    {
        SelectedTitle = null;
            
        var titles = await HonorificService.GetCharacterTitleList().ConfigureAwait(false);

        var final = new Dictionary<string, List<HonorificCustomTitle>>();
        foreach (var (worldId, dictionary) in titles)
        {
            if (_world.TryGetWorldById(worldId > ushort.MaxValue ? ushort.MaxValue : (ushort)worldId) is not { } worldName)
                continue;

            foreach (var (character, configuration) in dictionary)
                final[$"{character} - {worldName}"] = configuration;
        }

        _titles = final;
    }
    
    public bool MissingPermissionsForATarget()
    {
        foreach (var friend in _selection.Selected)
        {
            if (friend.PermissionsGrantedByFriend is null)
                continue;
            
            if ((friend.PermissionsGrantedByFriend.Primary & PrimaryPermissions.Honorific) is not PrimaryPermissions.Honorific)
                return true;
        }
        
        return false;
    }

    public async Task SendHonorific()
    {
        if (SelectedTitle == null)
            return;
            
        _commandLockout.Lock();
            
        var request = new HonorificRequest(_selection.GetSelectedFriendCodes(), SelectedTitle.ToHonorificDto());
        var response = await _network.InvokeAsync<ActionResponse>(HubMethod.Honorific, request).ConfigureAwait(false);
        ActionResponseParser.Parse("Honorific", response);
    }
    
    private void OnIpcReady(object? sender, EventArgs e)
    {
        _ = RefreshTitles().ConfigureAwait(false);
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
    private Dictionary<string, List<HonorificCustomTitle>> FilterTitles()
    {
        var result = new Dictionary<string, List<HonorificCustomTitle>>();
        foreach (var (character, titles) in _titles)
        {
            var list = new List<HonorificCustomTitle>();
            foreach (var title in titles)
                if (title.Title.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase))
                    list.Add(title);
            
            if (list.Count > 0)
                result.Add(character, list);
        }

        return result;
    }
}