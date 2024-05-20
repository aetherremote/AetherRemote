using System;

namespace AetherRemoteClient.UI.Tabs.Modules;

public interface IAetherRemoteModule : IDisposable
{
    public void Draw();
}
