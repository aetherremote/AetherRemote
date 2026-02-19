using System.Threading.Tasks;
using AetherRemoteClient.Services;

namespace AetherRemoteClient.UI.Views.Home;

public class HomeViewUiController(NetworkService networkService)
{
    public async Task Disconnect()
    {
        await networkService.StopAsync().ConfigureAwait(false);
    }
}