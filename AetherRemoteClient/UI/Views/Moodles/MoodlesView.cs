using System;
using AetherRemoteClient.Domain.Interfaces;
using AetherRemoteClient.Managers;
using AetherRemoteClient.Services;
using AetherRemoteClient.UI.Components.Friends;

namespace AetherRemoteClient.UI.Views.Moodles;

public partial class MoodlesView : IDisposable, IDrawable
{
    private readonly FriendsListComponentUi _friendsListComponentUi;
    private readonly CommandLockoutService _commandLockoutService;
    private readonly MoodlesService _moodlesService;
    private readonly NetworkService _networkService;
    private readonly NotesService _notesService;
    private readonly SelectionManager _selectionManager;
    
    public MoodlesView(
        FriendsListComponentUi friendsListComponentUi,
        CommandLockoutService commandLockoutService,
        NetworkService networkService, 
        NotesService notesService,
        MoodlesService moodlesService, 
        SelectionManager selectionManager)
    {
        _friendsListComponentUi = friendsListComponentUi;
        _commandLockoutService = commandLockoutService;
        _moodlesService = moodlesService;
        _networkService = networkService;
        _notesService = notesService;
        _selectionManager = selectionManager;
        
        _moodlesService.IpcReady += OnIpcReady;
        if (_moodlesService.ApiAvailable)
            RefreshMoodles();
    }
    
    public void Dispose()
    {
        _moodlesService.IpcReady -= OnIpcReady;
        GC.SuppressFinalize(this);
    }
}