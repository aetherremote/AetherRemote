using System;
using AetherRemoteClient.Domain.Interfaces;
using AetherRemoteClient.Managers;
using AetherRemoteClient.Services;
using AetherRemoteClient.UI.Components.Friends;

namespace AetherRemoteClient.UI.Views.Transformations;

public partial class TransformationsView : IDisposable, IDrawable
{
    private readonly FriendsListComponentUi _friendsListComponentUi;
    private readonly CharacterConfigurationService _characterConfigurationService;
    private readonly CommandLockoutService _commandLockoutService;
    private readonly GlamourerService _glamourerService;
    private readonly NetworkService _networkService;
    private readonly NotesService _notesService;
    private readonly StatusService _statusService;
    private readonly CharacterTransformationManager _characterTransformationManager;
    private readonly NetworkCommandManager _networkCommandManager;
    private readonly SelectionManager _selectionManager;
    
    public TransformationsView(
        FriendsListComponentUi friendsListComponentUi, 
        CharacterConfigurationService characterConfigurationService,
        CommandLockoutService commandLockoutService, 
        GlamourerService glamourerService, 
        NetworkService networkService,
        NotesService notesService,
        StatusService statusService,
        CharacterTransformationManager characterTransformationManager,
        NetworkCommandManager networkCommandManager, 
        SelectionManager selectionManager)
    {
        _friendsListComponentUi = friendsListComponentUi;
        _characterConfigurationService = characterConfigurationService;
        _commandLockoutService = commandLockoutService;
        _glamourerService = glamourerService;
        _networkService = networkService;
        _notesService = notesService;
        _statusService = statusService;
        _characterTransformationManager = characterTransformationManager;
        _networkCommandManager = networkCommandManager;
        _selectionManager = selectionManager;
        
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