using System;
using AetherRemoteClient.Domain;
using AetherRemoteClient.Domain.Interfaces;
using AetherRemoteClient.Managers;
using AetherRemoteClient.Services;
using AetherRemoteClient.UI.Components.Friends;

namespace AetherRemoteClient.UI.Views.CustomizePlus;

public partial class CustomizePlusView : IDisposable, IView
{
    // IView property
    public View View => View.CustomizePlus;

    // Injected
    private readonly FriendsListComponentUi _friendsListComponentUi;
    private readonly CommandLockoutService _commandLockoutService;
    private readonly CustomizePlusService _customizePlusService;
    private readonly NetworkCommandManager _networkCommandManager;
    private readonly SelectionManager _selectionManager;
    
    public CustomizePlusView(
        FriendsListComponentUi friendsList, 
        CommandLockoutService commandLockoutService, 
        CustomizePlusService customizePlusService,
        NetworkCommandManager networkCommandManager,
        SelectionManager selectionManager)
    {
        _friendsListComponentUi = friendsList;
        _commandLockoutService = commandLockoutService;
        _customizePlusService = customizePlusService;
        _networkCommandManager = networkCommandManager;
        _selectionManager = selectionManager;
    }
    
    public void Initialize()
    {
        _customizePlusService.IpcReady += OnIpcReady;
        if (_customizePlusService.ApiAvailable)
            _ = RefreshCustomizeProfiles();
    }
    
    public void Dispose()
    {
        _customizePlusService.IpcReady -= OnIpcReady;
        GC.SuppressFinalize(this);
    }
}