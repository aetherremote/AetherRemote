using System;
using System.Threading.Tasks;
using AetherRemoteClient.Domain.Events;
using AetherRemoteClient.Managers;
using AetherRemoteClient.Services;

namespace AetherRemoteClient.Handlers;

public class GlamourerEventHandler : IDisposable
{
    private readonly CustomizePlusService _customizePlusService;
    private readonly GlamourerService _glamourerService;
    private readonly PenumbraService _penumbraService;
    private readonly CharacterTransformationManager _characterTransformationManager;
    
    public GlamourerEventHandler(
        CustomizePlusService customizePlusService, 
        GlamourerService glamourerService, 
        PenumbraService penumbraService,
        CharacterTransformationManager characterTransformationManager)
    {
        _customizePlusService = customizePlusService;
        _glamourerService = glamourerService;
        _penumbraService = penumbraService;
        _characterTransformationManager = characterTransformationManager;
        
        _glamourerService.LocalPlayerResetOrReapply += OnLocalPlayerResetOrReapply;
    }

    private void OnLocalPlayerResetOrReapply(object? sender, GlamourerStateChangedEventArgs e)
    {
        _ = OnLocalPlayerResetOrReapplyAsync().ConfigureAwait(false);
    }

    private async Task OnLocalPlayerResetOrReapplyAsync()
    {
        await _customizePlusService.DeleteTemporaryCustomizeAsync().ConfigureAwait(false);
        
        if (_characterTransformationManager.TryGetCollectionThatHasAetherRemoteMods() is { } collectionThatHasAetherRemoteMods)
            await _penumbraService.RemoveTemporaryMod(collectionThatHasAetherRemoteMods).ConfigureAwait(false);
    }

    public void Dispose()
    {
        _glamourerService.LocalPlayerResetOrReapply -= OnLocalPlayerResetOrReapply;
        GC.SuppressFinalize(this);
    }
}