using AetherRemoteClient.Domain;
using AetherRemoteClient.Domain.Interfaces;
using AetherRemoteClient.Services;

namespace AetherRemoteClient.UI.Views.Pause;

public partial class PauseView : IView
{
    // IView property
    public View View => View.Pause;
    
    // Injected
    private readonly FriendsListService _friendsListService;
    private readonly PauseService _pauseService;
    
    public PauseView(
        FriendsListService friendsListService, 
        PauseService pauseService)
    {
        _friendsListService = friendsListService;
        _pauseService = pauseService;
    }
}