using System;
using AetherRemoteClient.Domain;
using AetherRemoteClient.Domain.Interfaces;
using AetherRemoteClient.Managers;
using AetherRemoteClient.Services;
using AetherRemoteClient.Services.Configuration;
using AetherRemoteClient.Services.Dependencies;
using AetherRemoteClient.UI.Components.Friends;

namespace AetherRemoteClient.UI.Views.Honorific;

public partial class HonorificView : IDisposable, IView
{
    // IView property
    public View View => View.Honorific;
    
    // Injected
    private readonly FriendsListComponentUi _friendsListComponentUi;
    private readonly CommandLockoutService _commandLockoutService;
    private readonly ConfigurationService _configurationService;
    private readonly HonorificService _honorificService;
    private readonly NetworkService _networkService;
    private readonly WorldService _worldService;
    private readonly SelectionManager _selectionManager;

    public HonorificView(
        FriendsListComponentUi friendsListComponentUi,
        CommandLockoutService commandLockoutService,
        ConfigurationService configurationService,
        HonorificService honorificService,
        NetworkService networkService,
        WorldService worldService,
        SelectionManager selectionManager)
    {
        _friendsListComponentUi = friendsListComponentUi;
        _commandLockoutService = commandLockoutService;
        _configurationService = configurationService;
        _honorificService = honorificService;
        _networkService = networkService;
        _worldService = worldService;
        _selectionManager = selectionManager;
    }

    public void Initialize()
    {
        _honorificService.IpcReady += OnIpcReady;
        if (_honorificService.ApiAvailable)
            _ = RefreshTitles().ConfigureAwait(false);
    }
    
    public void Dispose()
    {
        _honorificService.IpcReady -= OnIpcReady;
        GC.SuppressFinalize(this);
    }
}