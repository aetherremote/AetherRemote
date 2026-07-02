using AetherRemoteClient.Domain.Interfaces;
using AetherRemoteClient.Managers;
using AetherRemoteClient.Managers.Possession;
using AetherRemoteClient.Services;
using AetherRemoteClient.UI.Components.Friends;

namespace AetherRemoteClient.UI.Views.Possession;

public partial class PossessionView : IDrawable
{
    private readonly FriendsListComponentUi _friendsListComponentUi;
    private readonly AgreementsService _agreementsService;
    private readonly CommandLockoutService _commandLockoutService;
    private readonly PossessionManager _possessionManager;
    private readonly SelectionManager _selectionManager;
    
    public PossessionView(
        FriendsListComponentUi friendsListComponentUi,
        AgreementsService agreementsService,
        CommandLockoutService commandLockoutService,
        PossessionManager possessionManager,
        SelectionManager selectionManager)
    {
        _friendsListComponentUi = friendsListComponentUi;
        _agreementsService = agreementsService;
        _commandLockoutService = commandLockoutService;
        _possessionManager = possessionManager;
        _selectionManager = selectionManager;
    }
}