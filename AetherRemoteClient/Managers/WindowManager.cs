using System;
using AetherRemoteClient.Style;
using AetherRemoteClient.UI;
using AetherRemoteClient.Utils;
using Dalamud.Interface.Windowing;

namespace AetherRemoteClient.Managers;

public class WindowManager : IDisposable
{
    private readonly MainWindow _mainWindow;
    private readonly WindowSystem _windowSystem;

    public WindowManager(MainWindow mainWindow)
    {
        _mainWindow = mainWindow;
        _windowSystem = new WindowSystem("Aether Remote");
        _windowSystem.AddWindow(mainWindow);

#if DEBUG
        _mainWindow.IsOpen = true;
#endif

        Plugin.PluginInterface.UiBuilder.Draw += Draw;
        Plugin.PluginInterface.UiBuilder.OpenMainUi += OpenMainUi;
        Plugin.PluginInterface.UiBuilder.OpenConfigUi += OpenMainUi;
    }

    private void Draw()
    {
        AetherRemoteImGui.Push();
        _windowSystem.Draw();
        AetherRemoteImGui.Pop();
    }

    private void OpenMainUi()
    {
        _mainWindow.IsOpen = true;
    }

    public void Dispose()
    {
        Plugin.PluginInterface.UiBuilder.Draw -= Draw;
        Plugin.PluginInterface.UiBuilder.OpenMainUi -= OpenMainUi;
        Plugin.PluginInterface.UiBuilder.OpenConfigUi -= OpenMainUi;

        _windowSystem.RemoveAllWindows();

        GC.SuppressFinalize(this);
    }
}