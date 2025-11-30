using System;
using System.Timers;
using AetherRemoteClient.Dependencies.CustomizePlus.Services;
using AetherRemoteClient.Dependencies.Glamourer.Services;
using AetherRemoteClient.Dependencies.Penumbra.Services;
using AetherRemoteClient.Domain.Events;

namespace AetherRemoteClient.Handlers;

public class GlamourerEventHandler : IDisposable
{
    private readonly CustomizePlusService _customizePlusService;
    private readonly GlamourerService _glamourerService;
    private readonly PenumbraService _penumbraService;
    private readonly PermanentTransformationHandler _permanentTransformationHandler;

    private readonly Timer _batchLocalPlayerChangedEventsTimer = new(1000);
    
    public GlamourerEventHandler(
        CustomizePlusService customizePlusService, 
        GlamourerService glamourerService, 
        PenumbraService penumbraService,
        PermanentTransformationHandler permanentTransformationHandler)
    {
        _customizePlusService = customizePlusService;
        _glamourerService = glamourerService;
        _penumbraService = penumbraService;
        _permanentTransformationHandler = permanentTransformationHandler;
        
        _glamourerService.LocalPlayerResetOrReapply += OnLocalPlayerResetOrReapply;
        // _glamourerService.LocalPlayerChanged += OnLocalPlayerChanged; // TODO: Re-enable when permanent transformations are a thing

        _batchLocalPlayerChangedEventsTimer.AutoReset = false;
        _batchLocalPlayerChangedEventsTimer.Elapsed += OnBatchedLocalPlayerChanged;
    }
    
    // ReSharper disable once UnusedMember.Local
    // ReSharper disable UnusedParameter.Local
    private void OnLocalPlayerChanged(object? sender, EventArgs e)
    {
        // Use a timer to batch all the changes. Helpful when a bunch of events are applied all at the same time as
        // not to flood other services / systems with extra work. This method could be expanded to batch all the
        // Glamourer event types too (with a change to the underlying event as well) to provide more robust knowledge
        _batchLocalPlayerChangedEventsTimer.Stop();
        _batchLocalPlayerChangedEventsTimer.Start();
    }
    
    private async void OnBatchedLocalPlayerChanged(object? sender, ElapsedEventArgs e)
    {
        try
        {
            await _permanentTransformationHandler.ResolveDifferencesAfterGlamourerUpdate();
        }
        catch (Exception exception)
        {
            Plugin.Log.Error($"[GlamourerEventHandler] Unknown exception on batched local player changed event, {exception}");
        }
    }

    private async void OnLocalPlayerResetOrReapply(object? sender, GlamourerStateChangedEventArgs e)
    {
        try
        {
            // Clean up the created CustomizePlus resources
            await _customizePlusService.DeleteTemporaryCustomizeAsync();
            
            // Clean up the temporary mods added to the collection
            var currentCollection = await _penumbraService.GetCollection().ConfigureAwait(false);
            await _penumbraService.CallRemoveTemporaryMod(currentCollection).ConfigureAwait(false);
        }
        catch (Exception exception)
        {
            Plugin.Log.Warning($"[GlamourerEventHandler] Unexpected error while deleting plugin resources on reset, {exception}");
        }
    }

    public void Dispose()
    {
        _glamourerService.LocalPlayerResetOrReapply -= OnLocalPlayerResetOrReapply;
        GC.SuppressFinalize(this);
    }
}