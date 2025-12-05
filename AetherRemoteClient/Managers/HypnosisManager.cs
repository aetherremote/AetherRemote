using System;
using System.Numerics;
using System.Threading.Tasks;
using AetherRemoteClient.Domain;
using AetherRemoteClient.Domain.Hypnosis;
using AetherRemoteCommon.Domain;
using Dalamud.Bindings.ImGui;

namespace AetherRemoteClient.Managers;

/// <summary>
///     Manages the rendering of a spiral and accompanying text to a client's screen
/// </summary>
public class HypnosisManager : IDisposable
{
    // Instantiated
    private readonly HypnosisRenderer _hypnosisRenderer = new();
    
    // Hypnosis data
    private HypnosisData? _hypnosisData;
    private Vector2 _screenSize;
    // private Vector2 _spiralScreenSize;
    
    /// <summary>
    ///     If the client is being hypnotized
    /// </summary>
    public bool IsBeingHypnotized => _hypnosisData is not null;

    /// <summary>
    ///     Who is currently hypnotizing the client, empty if no one
    /// </summary>
    public Friend? Hypnotist;

    /// <summary>
    ///     <inheritdoc cref="HypnosisManager"/>
    /// </summary>
    public HypnosisManager()
    {
        Plugin.PluginInterface.UiBuilder.Draw += OnDraw;
    }

    /// <summary>
    ///     Begin hypnotizing the client
    /// </summary>
    public async Task Hypnotize(Friend hypnotist, HypnosisData hypnosisData)
    {
        // Hypnotist
        Hypnotist = hypnotist;
        
        // Screen data
        _screenSize = ImGui.GetIO().DisplaySize;
        
        // Set all relevant fields
        await _hypnosisRenderer.SetRendererFromHypnosisData(hypnosisData, _screenSize).ConfigureAwait(false);
        
        // Save
        _hypnosisData = hypnosisData;
    }

    /// <summary>
    ///     Stops hypnotizing the client
    /// </summary>
    public bool Wake()
    {
        _hypnosisData = null;
        return true;
    }
    
    private void OnDraw()
    {
        if (_hypnosisData is null)
            return;
        
        var draw = ImGui.GetForegroundDrawList();
        _hypnosisRenderer.Render(draw, _screenSize, Vector2.Zero);
    }

    public void Dispose()
    {
        Plugin.PluginInterface.UiBuilder.Draw -= OnDraw;
        GC.SuppressFinalize(this);
    }
}