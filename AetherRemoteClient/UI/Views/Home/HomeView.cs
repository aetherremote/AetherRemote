using AetherRemoteClient.Domain;
using AetherRemoteClient.Domain.Interfaces;
using AetherRemoteClient.Services;

namespace AetherRemoteClient.UI.Views.Home;

public partial class HomeView : IView
{
    // IView property
    public View View => View.Home;
    
    // Injected
    private readonly AccountService _accountService;
    private readonly NetworkService _networkService;
    private readonly TipService _tipService;
    
    public HomeView(
        AccountService accountService,
        NetworkService networkService,
        TipService tipService)
    {
        _accountService = accountService;
        _networkService = networkService;
        _tipService = tipService;
    }
}