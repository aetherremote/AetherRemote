using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

// ReSharper disable MemberCanBeMadeStatic.Global

namespace AetherRemoteClient.UI.Views.Debug;

[SuppressMessage("Performance", "CA1822:Mark members as static")]
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