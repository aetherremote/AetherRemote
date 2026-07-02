using System.Threading.Tasks;

namespace AetherRemoteClient.UI.Views.Home;

public partial class HomeView
{
    // Control the draw state of the tutorial window
    private bool _showTutorialWindow;
    
    private async Task Disconnect()
    {
        await _networkService.DisconnectFromServerAsync().ConfigureAwait(false);
    }
}