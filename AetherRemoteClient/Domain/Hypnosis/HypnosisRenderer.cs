using System;
using System.Numerics;
using System.Threading.Tasks;
using AetherRemoteClient.Domain.Hypnosis.Components;
using AetherRemoteCommon.Domain;
using Dalamud.Bindings.ImGui;

namespace AetherRemoteClient.Domain.Hypnosis;

public class HypnosisRenderer : IDisposable
{
    // Renderers
    public readonly HypnosisSpiralRenderer Spiral = new();
    public readonly HypnosisTextRenderer Text = new();
    
    /// <summary>
    ///     Draw the spiral
    /// </summary>
    public void Render(ImDrawListPtr draw, Vector2 screenSize, Vector2 screenPosition)
    {
        Spiral.Render(draw, screenSize, screenPosition);
        Text.Render(draw, screenSize, screenPosition);
    }

    /// <summary>
    ///     Sets all relevant hypnosis data in the spiral and text renderer
    /// </summary>
    public async Task SetRendererFromHypnosisData(HypnosisData data, Vector2 windowSize)
    {
        Spiral.SetColor(data.SpiralColor);
        Spiral.SetDirection(data.SpiralDirection);
        Spiral.SetSpeed(data.SpiralSpeed);
        await Spiral.SetSpiral(data.SpiralArms, data.SpiralTurns, data.SpiralCurve, data.SpiralThickness).ConfigureAwait(false);
            
        Text.SetColor(data.TextColor);
        Text.SetDelay(data.TextDelay * 1000);
        Text.SetDuration(data.TextDuration * 1000);
        Text.SetMode(data.TextMode);
        await Text.SetText(data.TextWords, windowSize).ConfigureAwait(false);
    }

    public void Dispose()
    {
        Spiral.Dispose();
        GC.SuppressFinalize(this);
    }
}
