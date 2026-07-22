using AetherRemoteClient.Domain;
using AetherRemoteClient.Domain.Interfaces;
using AetherRemoteClient.Managers;
using AetherRemoteClient.Services;
using AetherRemoteClient.Services.Configuration;
using AetherRemoteClient.UI.Components.Friends;

namespace AetherRemoteClient.UI.Views.Possession;

public partial class PossessionView : IView
{
    // IView property
    public View View => View.Possession;
    
    // Injected
    private readonly FriendsListComponentUi _friendsListComponentUi;
    private readonly CommandLockoutService _commandLockoutService;
    private readonly ConfigurationService _configurationService;
    private readonly SelectionManager _selectionManager;
    
    public PossessionView(
        FriendsListComponentUi friendsListComponentUi,
        CommandLockoutService commandLockoutService,
        ConfigurationService configurationService,
        SelectionManager selectionManager)
    {
        _friendsListComponentUi = friendsListComponentUi;
        _commandLockoutService = commandLockoutService;
        _configurationService = configurationService;
        _selectionManager = selectionManager;
    }
}