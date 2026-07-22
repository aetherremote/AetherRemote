using System;
using AetherRemoteClient.Domain;
using AetherRemoteClient.Domain.Interfaces;
using AetherRemoteClient.Managers;
using AetherRemoteClient.Services;
using AetherRemoteClient.UI.Components.Friends;

namespace AetherRemoteClient.UI.Views.Emote;

public partial class EmoteView : IDisposable, IView
{
    // IView property
    public View View => View.Emote;
    
    // Injected
    private readonly FriendsListComponentUi _friendsList;
    private readonly CommandLockoutService _commandLockoutService;
    private readonly EmoteService _emoteService;
    private readonly NetworkRequestManager _networkRequestManager;
    private readonly SelectionManager _selectionManager;
    
    public EmoteView(
        FriendsListComponentUi friendsList,
        CommandLockoutService commandLockoutService,
        EmoteService emoteService,
        NetworkRequestManager networkRequestManager,
        SelectionManager selectionManager)
    {
        _friendsList = friendsList;
        _commandLockoutService = commandLockoutService;
        _emoteService = emoteService;
        _networkRequestManager = networkRequestManager;
        _selectionManager = selectionManager;
        
        _emotesListFilter = new ListFilter<string>(_emoteService.Emotes, FilterEmote);
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }
}