using AetherRemoteClient.Domain.Interfaces;
using AetherRemoteClient.Services;

namespace AetherRemoteClient.UI.Views.Home;

public partial class HomeView : IDrawable
{
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