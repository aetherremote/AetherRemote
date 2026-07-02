using System;
using AetherRemoteClient.Domain.Interfaces;
using AetherRemoteClient.Managers;
using AetherRemoteClient.Services;
using AetherRemoteClient.UI.Components.Friends;

namespace AetherRemoteClient.UI.Views.Friends;

public partial class FriendsView : IDisposable, IDrawable
{
    private readonly FriendsListComponentUi _friendsListComponentUi;
    private readonly AccountService _accountService;
    private readonly FriendsListService _friendsListService;
    private readonly NetworkService _networkService;
    private readonly NotesService _notesService;
    private readonly SelectionManager _selectionManager;
    
    public FriendsView(
        FriendsListComponentUi friendsListComponentUi,
        AccountService accountService,
        FriendsListService friendsListService,
        NetworkService networkService,
        NotesService notesService,
        SelectionManager selectionManager)
    {
        _friendsListComponentUi = friendsListComponentUi;
        _accountService = accountService;
        _friendsListService = friendsListService;
        _networkService = networkService;
        _notesService = notesService;
        _selectionManager = selectionManager;
        
        _accountService.GlobalPermissionsUpdated += OnGlobalPermissionsUpdated;
        _selectionManager.FriendSelected += OnFriendSelected;
    }

    public void Dispose()
    {
        _accountService.GlobalPermissionsUpdated -= OnGlobalPermissionsUpdated;
        _selectionManager.FriendSelected -= OnFriendSelected;
        GC.SuppressFinalize(this);
    }
}