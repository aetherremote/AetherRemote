using System;
using System.Numerics;
using AetherRemoteClient.Managers;
using AetherRemoteClient.Services;
using AetherRemoteClient.UI.Components.NavigationBar;
using AetherRemoteClient.UI.Views;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Windowing;

namespace AetherRemoteClient.UI;

public class MainWindow : Window, IDisposable
{
    // Const
    private static readonly string MainWindowTitle = $"Aether Remote 2 - Version {Plugin.Version}";

    // Injected
    private readonly NavigationBarComponentUi _navigationBarComponentUi;
    private readonly ViewRegistry _viewRegistry;
    private readonly ViewService _viewService;
    private readonly DtrManager _dtrManager;
    
    public MainWindow(
        NavigationBarComponentUi navigationBarComponentUi,
        ViewRegistry viewRegistry,
        ViewService viewService,
        DtrManager dtrManager) : base(MainWindowTitle)
    {
        SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new Vector2(800, 500),
            MaximumSize = ImGui.GetIO().DisplaySize
        };
        
        _navigationBarComponentUi = navigationBarComponentUi;
        _viewRegistry = viewRegistry;
        _viewService = viewService;
        _dtrManager = dtrManager;
        
        _dtrManager.DtrClicked += OnDtrClicked;
    }

    public override void Draw()
    {
        _navigationBarComponentUi.Draw();

        ImGui.SameLine();

        _viewRegistry.Get(_viewService.CurrentView).Draw();
    }
    
    private void OnDtrClicked()
    {
        IsOpen = true;
    }

    public void Dispose()
    {
        _dtrManager.DtrClicked -= OnDtrClicked;
        GC.SuppressFinalize(this);
    }
}