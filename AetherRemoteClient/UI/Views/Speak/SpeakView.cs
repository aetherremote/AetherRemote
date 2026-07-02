using System;
using System.Linq;
using AetherRemoteClient.Domain;
using AetherRemoteClient.Domain.Interfaces;
using AetherRemoteClient.Managers;
using AetherRemoteClient.Services;
using AetherRemoteClient.UI.Components.Friends;
using AetherRemoteCommon.Domain.Enums;
using AetherRemoteCommon.Util;

namespace AetherRemoteClient.UI.Views.Speak;

public partial class SpeakView : IView
{
    // IView property
    public View View => View.Speak;
    
    // Injected
    private readonly FriendsListComponentUi _friendsListComponentUi;
    private readonly CommandLockoutService _commandLockoutService;
    private readonly WorldService _worldService;
    private readonly NetworkCommandManager _networkCommandManager;
    private readonly SelectionManager _selectionManager;
    
    public SpeakView(
        FriendsListComponentUi friendsListComponentUi,
        CommandLockoutService commandLockoutService,
        WorldService worldService, 
        NetworkCommandManager networkCommandManager,
        SelectionManager selectionManager)
    {
        _friendsListComponentUi = friendsListComponentUi;
        _commandLockoutService = commandLockoutService;
        _worldService = worldService;
        _networkCommandManager = networkCommandManager;
        _selectionManager = selectionManager;
        
        _worldsListFilter = new ListFilter<string>(worldService.WorldNames, FilterWorld);
        _chatModeOptions = (from ChatChannel mode in Enum.GetValues<ChatChannel>() select mode.Beautify()).ToArray();
    }
}