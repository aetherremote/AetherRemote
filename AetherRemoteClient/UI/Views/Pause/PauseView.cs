using AetherRemoteClient.Domain.Interfaces;
using AetherRemoteClient.Services;

namespace AetherRemoteClient.UI.Views.Pause;

public partial class PauseView : IDrawable
{
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