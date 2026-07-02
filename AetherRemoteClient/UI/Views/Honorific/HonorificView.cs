using System;
using AetherRemoteClient.Domain.Interfaces;
using AetherRemoteClient.Managers;
using AetherRemoteClient.Services;
using AetherRemoteClient.UI.Components.Friends;

namespace AetherRemoteClient.UI.Views.Honorific;

public partial class HonorificView : IDisposable, IDrawable
{
    private readonly FriendsListComponentUi _friendsListComponentUi;
    private readonly CommandLockoutService _commandLockoutService;
    private readonly HonorificService _honorificService;
    private readonly NetworkService _networkService;
    private readonly NotesService _notesService;
    private readonly WorldService _worldService;
    private readonly SelectionManager _selectionManager;

    public HonorificView(
        FriendsListComponentUi friendsListComponentUi,
        CommandLockoutService commandLockoutService,
        HonorificService honorificService,
        NetworkService networkService,
        NotesService notesService,
        WorldService worldService,
        SelectionManager selectionManager)
    {
        _friendsListComponentUi = friendsListComponentUi;
        _commandLockoutService = commandLockoutService;
        _honorificService = honorificService;
        _networkService = networkService;
        _notesService = notesService;
        _worldService = worldService;
        _selectionManager = selectionManager;
        
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