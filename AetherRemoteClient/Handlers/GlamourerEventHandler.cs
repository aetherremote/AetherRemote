using System;
using System.Timers;
using AetherRemoteClient.Dependencies;
using AetherRemoteClient.Domain.Events;
using AetherRemoteClient.Managers;

namespace AetherRemoteClient.Handlers;

public class GlamourerEventHandler : IDisposable
{
    private readonly CustomizePlusDependency _customizePlusDependency;
    private readonly GlamourerDependency _glamourerDependency;
    private readonly PenumbraDependency _penumbraDependency;
    private readonly PermanentTransformationManager _permanentTransformationManager;

    private readonly Timer _batchLocalPlayerChangedEventsTimer = new(1000);
    
    public GlamourerEventHandler(
        CustomizePlusDependency customizePlusDependency, 
        GlamourerDependency glamourerDependency, 
        PenumbraDependency penumbraDependency,
        PermanentTransformationManager permanentTransformationManager)
    {
        _customizePlusDependency = customizePlusDependency;
        _glamourerDependency = glamourerDependency;
        _penumbraDependency = penumbraDependency;
        _permanentTransformationManager = permanentTransformationManager;
        
        _glamourerDependency.LocalPlayerResetOrReapply += OnLocalPlayerResetOrReapply;
        _glamourerDependency.LocalPlayerChanged += OnLocalPlayerChanged;

        _batchLocalPlayerChangedEventsTimer.AutoReset = false;
        _batchLocalPlayerChangedEventsTimer.Elapsed += OnBatchedLocalPlayerChanged;
    }
    
    private void OnLocalPlayerChanged(object? sender, EventArgs e)
    {
        // Use a timer to batch all the changes. Helpful when a bunch of events are applied all at the same time as
        // not to flood other services / systems with extra work. This method could be expanded to batch all the
        // Glamourer event types too (with a change to the underlying event as well) to provide more robust knowledge
        _batchLocalPlayerChangedEventsTimer.Stop();
        _batchLocalPlayerChangedEventsTimer.Start();
        // TODO: Test this
    }
    
    private void OnBatchedLocalPlayerChanged(object? sender, ElapsedEventArgs e)
    {
        _permanentTransformationManager.ResolveDifferencesAfterGlamourerUpdate();
    }

    private async void OnLocalPlayerResetOrReapply(object? sender, GlamourerStateChangedEventArgs e)
    {
        try
        {
            // Clean up the created CustomizePlus resources
            await _customizePlusDependency.DeleteCustomize();
            
            // Clean up the temporary mods added to the collection
            var currentCollection = await _penumbraDependency.GetCollection().ConfigureAwait(false);
            await _penumbraDependency.CallRemoveTemporaryMod(currentCollection).ConfigureAwait(false);
        }
        catch (Exception exception)
        {
            Plugin.Log.Warning($"[GlamourerEventHandler] Unexpected error while deleting plugin resources on reset, {exception}");
        }
    }

    public void Dispose()
    {
        _glamourerDependency.LocalPlayerResetOrReapply -= OnLocalPlayerResetOrReapply;
        GC.SuppressFinalize(this);
    }
}