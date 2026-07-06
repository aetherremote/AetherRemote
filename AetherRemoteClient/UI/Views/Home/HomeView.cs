using AetherRemoteClient.Domain;
using AetherRemoteClient.Domain.Interfaces;
using AetherRemoteClient.Services;

namespace AetherRemoteClient.UI.Views.Home;

public partial class HomeView : IView
{
    // IView property
    public View View => View.Home;
    
    // Injected
    private readonly ActiveSessionService _activeSessionService;
    private readonly NetworkService _networkService;
    private readonly TipService _tipService;
    
    public HomeView(
        ActiveSessionService activeSessionService,
        NetworkService networkService,
        TipService tipService)
    {
        _activeSessionService = activeSessionService;
        _networkService = networkService;
        _tipService = tipService;
    }
}