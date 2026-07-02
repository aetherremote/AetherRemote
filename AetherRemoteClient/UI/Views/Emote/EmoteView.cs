using System;
using AetherRemoteClient.Domain;
using AetherRemoteClient.Domain.Interfaces;
using AetherRemoteClient.Managers;
using AetherRemoteClient.Services;
using AetherRemoteClient.UI.Components.Friends;

namespace AetherRemoteClient.UI.Views.Emote;

public partial class EmoteView : IDisposable, IDrawable
{
    private readonly FriendsListComponentUi _friendsList;
    private readonly CommandLockoutService _commandLockoutService;
    private readonly EmoteService _emoteService;
    private readonly NetworkCommandManager _networkCommandManager;
    private readonly SelectionManager _selectionManager;
    
    public EmoteView(
        FriendsListComponentUi friendsList,
        CommandLockoutService commandLockoutService,
        EmoteService emoteService,
        NetworkCommandManager networkCommandManager,
        SelectionManager selectionManager)
    {
        _friendsList = friendsList;
        _commandLockoutService = commandLockoutService;
        _emoteService = emoteService;
        _networkCommandManager = networkCommandManager;
        _selectionManager = selectionManager;
        
        _emotesListFilter = new ListFilter<string>(_emoteService.Emotes, FilterEmote);
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }
}