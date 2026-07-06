using System;
using AetherRemoteClient.Domain;
using AetherRemoteClient.Domain.Interfaces;
using AetherRemoteClient.Managers;
using AetherRemoteClient.Services;
using AetherRemoteClient.Services.Configuration;
using AetherRemoteClient.UI.Components.Friends;

namespace AetherRemoteClient.UI.Views.Friends;

public partial class FriendsView : IDisposable, IView
{
    // IView property
    public View View => View.Friends;
    
    // Injected
    private readonly FriendsListComponentUi _friendsListComponentUi;
    private readonly ActiveSessionService _activeSessionService;
    private readonly ConfigurationService _configurationService;
    private readonly FriendsListService _friendsListService;
    private readonly NetworkService _networkService;
    private readonly SelectionManager _selectionManager;
    
    public FriendsView(
        FriendsListComponentUi friendsListComponentUi,
        ActiveSessionService activeSessionService,
        ConfigurationService configurationService,
        FriendsListService friendsListService,
        NetworkService networkService,
        SelectionManager selectionManager)
    {
        _friendsListComponentUi = friendsListComponentUi;
        _activeSessionService = activeSessionService;
        _configurationService = configurationService;
        _friendsListService = friendsListService;
        _networkService = networkService;
        _selectionManager = selectionManager;

        _activeSessionService.GlobalPermissionsChanged += OnGlobalPermissionsChanged;
        _selectionManager.FriendSelected += OnFriendSelected;
    }

    public void Dispose()
    {
        _selectionManager.FriendSelected -= OnFriendSelected;
        GC.SuppressFinalize(this);
    }
}