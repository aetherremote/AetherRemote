using System.Threading.Tasks;

namespace AetherRemoteClient.UI.Views.Debug;

public class DebugViewUiController
{
    public async Task Debug()
    {
        // Do Something
        await Task.Delay(1000).ConfigureAwait(false);
    }
    
    public async Task Debug2()
    {
        // Do Something
        await Task.Delay(1000).ConfigureAwait(false);
    }
}