using System;
using AetherRemoteClient.Domain;
using AetherRemoteClient.Domain.Interfaces;
using AetherRemoteClient.Managers;
using AetherRemoteClient.Services;
using AetherRemoteClient.Services.Configuration;
using AetherRemoteClient.Services.Dependencies;
using AetherRemoteClient.UI.Components.Friends;

namespace AetherRemoteClient.UI.Views.Moodles;

public partial class MoodlesView : IDisposable, IView
{
    // IView property
    public View View => View.Moodles;
    
    // Injected
    private readonly FriendsListComponentUi _friendsListComponentUi;
    private readonly CommandLockoutService _commandLockoutService;
    private readonly MoodlesService _moodlesService;
    private readonly NetworkService _networkService;
    private readonly SelectionManager _selectionManager;
    
    public MoodlesView(
        FriendsListComponentUi friendsListComponentUi,
        CommandLockoutService commandLockoutService,
        NetworkService networkService, 
        MoodlesService moodlesService, 
        SelectionManager selectionManager)
    {
        _friendsListComponentUi = friendsListComponentUi;
        _commandLockoutService = commandLockoutService;
        _moodlesService = moodlesService;
        _networkService = networkService;
        _selectionManager = selectionManager;
    }

    public void Initialize()
    {
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