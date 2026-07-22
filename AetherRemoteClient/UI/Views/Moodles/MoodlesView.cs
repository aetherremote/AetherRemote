using System;
using AetherRemoteClient.Domain;
using AetherRemoteClient.Domain.Interfaces;
using AetherRemoteClient.Managers;
using AetherRemoteClient.Services;
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
    private readonly NetworkRequestManager _networkRequestManager;
    private readonly SelectionManager _selectionManager;
    
    public MoodlesView(
        FriendsListComponentUi friendsListComponentUi,
        CommandLockoutService commandLockoutService,
        MoodlesService moodlesService, 
        NetworkRequestManager networkRequestManager,
        SelectionManager selectionManager)
    {
        _friendsListComponentUi = friendsListComponentUi;
        _commandLockoutService = commandLockoutService;
        _moodlesService = moodlesService;
        _networkRequestManager = networkRequestManager;
        _selectionManager = selectionManager;
    }

    public void Initialize()
    {
        _moodlesService.IpcReady += OnIpcReady;
        if (_moodlesService.ApiAvailable)
            _ = RefreshMoodles().ConfigureAwait(false);
    }
    
    public void Dispose()
    {
        _moodlesService.IpcReady -= OnIpcReady;
        GC.SuppressFinalize(this);
    }
}