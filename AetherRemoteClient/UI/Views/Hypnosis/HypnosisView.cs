using System;
using AetherRemoteClient.Domain;
using AetherRemoteClient.Domain.Interfaces;
using AetherRemoteClient.Managers;
using AetherRemoteClient.Services;
using AetherRemoteClient.UI.Components.Friends;

namespace AetherRemoteClient.UI.Views.Hypnosis;

public partial class HypnosisView : IDisposable, IView
{
    // IView property
    public View View => View.Hypnosis;
    
    // Injected
    private readonly FriendsListComponentUi _friendsListComponentUi;
    private readonly CommandLockoutService _commandLockoutService;
    private readonly NetworkRequestManager _networkRequestManager;
    private readonly SelectionManager _selectionManager;
    
    public HypnosisView(
        FriendsListComponentUi friendsListComponentUi,
        CommandLockoutService commandLockoutService,
        NetworkRequestManager networkRequestManager,
        SelectionManager selectionManager)
    {
        _friendsListComponentUi = friendsListComponentUi;
        _commandLockoutService = commandLockoutService;
        _networkRequestManager = networkRequestManager;
        _selectionManager = selectionManager;
        
        _spiralRefreshCooldown.AutoReset = false;
        _spiralRefreshCooldown.Enabled = false;
        _spiralRefreshCooldown.Elapsed += OnRefreshSpiral;

        _textRefreshCooldown.AutoReset = false;
        _textRefreshCooldown.Enabled = false;
        _textRefreshCooldown.Elapsed += OnRefreshText;

        _saveLoadSpiralFileOptionsListFilter = new ListFilter<string>(_saveLoadSpiralFileOptions, (spiralName, searchTerm) => spiralName.Contains(searchTerm, StringComparison.OrdinalIgnoreCase));

        RefreshSavedSpiralFileNames();
        
        BeginSpiralRefreshTimer();
    }

    public void Dispose()
    {
        // Dispose of the spiral timer
        _spiralRefreshCooldown.Elapsed += OnRefreshSpiral;
        _spiralRefreshCooldown.Dispose();
        
        // Dispose of the text timer
        _textRefreshCooldown.Elapsed -= OnRefreshText;
        _textRefreshCooldown.Dispose();
        
        // Dispose of the textures created in the hypnosis renderer
        _hypnosisRenderer.Dispose();
        GC.SuppressFinalize(this);
    }
}