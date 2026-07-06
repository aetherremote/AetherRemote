using System;
using AetherRemoteClient.Domain;
using AetherRemoteClient.Domain.Interfaces;
using AetherRemoteClient.Managers;
using AetherRemoteClient.Services;
using AetherRemoteClient.Services.Dependencies;
using AetherRemoteClient.UI.Components.Friends;

namespace AetherRemoteClient.UI.Views.Transformations;

public partial class TransformationsView : IDisposable, IView
{
    // IView property
    public View View => View.Transformations;
    
    // Injected
    private readonly FriendsListComponentUi _friendsListComponentUi;
    private readonly ActiveSessionService _activeSessionService;
    private readonly CommandLockoutService _commandLockoutService;
    private readonly GlamourerService _glamourerService;
    private readonly NetworkService _networkService;
    private readonly StatusService _statusService;
    private readonly CharacterTransformationManager _characterTransformationManager;
    private readonly NetworkCommandManager _networkCommandManager;
    private readonly SelectionManager _selectionManager;
    
    public TransformationsView(
        FriendsListComponentUi friendsListComponentUi, 
        ActiveSessionService activeSessionService,
        CommandLockoutService commandLockoutService, 
        GlamourerService glamourerService, 
        NetworkService networkService,
        StatusService statusService,
        CharacterTransformationManager characterTransformationManager,
        NetworkCommandManager networkCommandManager, 
        SelectionManager selectionManager)
    {
        _friendsListComponentUi = friendsListComponentUi;
        _activeSessionService = activeSessionService;
        _commandLockoutService = commandLockoutService;
        _glamourerService = glamourerService;
        _networkService = networkService;
        _statusService = statusService;
        _characterTransformationManager = characterTransformationManager;
        _networkCommandManager = networkCommandManager;
        _selectionManager = selectionManager;
    }
    
    public void Initialize()
    {
        _glamourerService.IpcReady += OnIpcReady;
        if (_glamourerService.ApiAvailable)
            _ = RefreshGlamourerDesigns().ConfigureAwait(false);
    }
    
    public void Dispose()
    {
        _glamourerService.IpcReady -= OnIpcReady;
        GC.SuppressFinalize(this);
    }
}