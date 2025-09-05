using System;
using System.Collections.Generic;
using System.Numerics;
using System.Threading.Tasks;
using AetherRemoteClient.Utils;
using AetherRemoteCommon.Domain.Enums;
using Dalamud.Bindings.ImGui;

namespace AetherRemoteClient.Domain.Hypnosis.Components;

public class HypnosisTextRenderer
{
    // Const
    private static readonly Random Random = new();
    
    // Defaults
    public const uint DefaultTextColor = 0xFFFFFFFF;
    public const int DefaultTextDelayInMilliseconds = 2000;
    public const int DefaultTextDurationInMilliseconds = 3000;
    public const HypnosisTextMode DefaultHypnosisTextMode = HypnosisTextMode.Sequential;
    
    // Private values
    private int _elapsedTime;
    private int _index;
    
    // Configuration values
    private uint _color = DefaultTextColor;
    private int _delayInMilliseconds = DefaultTextDelayInMilliseconds;
    private int _durationInMilliseconds = DefaultTextDurationInMilliseconds;
    private HypnosisTextMode _mode = DefaultHypnosisTextMode;
    private List<HypnosisText> _texts = [];
    
    // Set properties
    public void SetColor(uint color) => _color = color;
    public void SetColor(Vector4 color) => _color = ImGui.ColorConvertFloat4ToU32(color);

    public void SetDelay(int delayInMilliseconds) => _delayInMilliseconds = delayInMilliseconds;
    public void SetDuration(int durationInMilliseconds) => _durationInMilliseconds = durationInMilliseconds is 0 ? 100 : durationInMilliseconds;
    public void SetMode(HypnosisTextMode mode) => _mode = mode;
    public async Task SetText(string[] lines, Vector2 screenSize)
    {
        _texts = await HypnosisTextGenerator.ComputeTextSizes(SharedUserInterfaces.MassiveFont, screenSize, lines).ConfigureAwait(false);

        _index = 0;
        _elapsedTime = 0;
    }
    
    // Render
    public void Render(ImDrawListPtr draw, Vector2 size, Vector2 screenPosition)
    {
        // Only render if we have things present
        if (_texts.Count < 1)
            return;
        
        // Update the elapsed time
        _elapsedTime += Plugin.Framework.UpdateDelta.Milliseconds;

        // If we're still drawing
        if (_texts.Count is 1 || _elapsedTime < _durationInMilliseconds)
        {
            var current = _texts[_index];
            var position = screenPosition + (size - current.BoundingSize) * 0.5f;

            foreach (var line in current.Lines)
            {
                var final = position - new Vector2(line.Size.X * 0.5f, 0);
                draw.AddText(SharedUserInterfaces.MassiveFont,  current.FontSize, final, _color, line.Text);
                position.Y += line.Size.Y;
            }

            return;
        }

        // If we're still resting
        if (_delayInMilliseconds is not 0 && _elapsedTime - _durationInMilliseconds < _delayInMilliseconds)
            return;
        
        // Reset
        _elapsedTime = 0;

        // If it's only one, return since it'll be the only option
        if (_texts.Count is 1)
            return;

        // Sequential
        if (_mode is HypnosisTextMode.Sequential)
        {
            _index++;
            _index %= _texts.Count;
        }
        else // Random
        {
            var next = Random.Next(0, _texts.Count);
            while (next == _index)
                next = Random.Next(0, _texts.Count);

            _index = next;
        }
    }
}