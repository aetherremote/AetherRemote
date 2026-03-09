using System;
using AetherRemoteClient.Dependencies.Glamourer.Services;
using AetherRemoteClient.Domain.Enums;
using AetherRemoteClient.Managers;

namespace AetherRemoteClient.UI.Views.Transformations.Controllers;

public partial class TransformationsViewUiController : IDisposable
{
    // Injected
    private readonly GlamourerService _glamourerService;
    private readonly NetworkCommandManager _networkCommandManager;
    private readonly SelectionManager _selectionManager;

    public TransformationsViewUiController(GlamourerService glamourer, NetworkCommandManager networkCommandManager, SelectionManager selectionManager)
    {
        _glamourerService = glamourer;
        _networkCommandManager = networkCommandManager;
        _selectionManager = selectionManager;

        _glamourerService.IpcReady += OnIpcReady;
        if (_glamourerService.ApiAvailable)
            _ = RefreshGlamourerDesigns();
    }
    
    /// <summary>
    ///     What mode the Ui will display, and how network events will be sent
    /// </summary>
    public TransformationMode Mode = TransformationMode.Transform;
    
    private void OnIpcReady(object? sender, EventArgs e)
    {
        _ = RefreshGlamourerDesigns().ConfigureAwait(false);
    }
    
    public void Dispose()
    {
        _glamourerService.IpcReady -= OnIpcReady;
        GC.SuppressFinalize(this);
    }
}