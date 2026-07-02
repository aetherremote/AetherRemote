using System.Threading.Tasks;

namespace AetherRemoteClient.UI.Views.Debug;

public partial class DebugView
{
    private async Task Debug()
    {
        // Do Something
        await Task.Delay(1000).ConfigureAwait(false);
    }

    private async Task Debug2()
    {
        // Do Something
        await Task.Delay(1000).ConfigureAwait(false);
    }
}