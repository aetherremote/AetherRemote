using System;
using System.IO;
using System.Numerics;
using System.Timers;
using AetherRemoteClient.Utils;
using ImGuiNET;

namespace AetherRemoteClient.UI.Views.Hypnosis;

public class HypnosisViewUiController
{
    // TODO: Organize all variables and group them together
    
    private const float SpiralSpeedMax = 0.003f;
    private const float SpiralSpeedMin = 0;
    private const string SpiralName = "a.png";
    private const float SpiralScalar = 0.5f * SpiralPreviewZoom;
    private const float SpiralPreviewZoom = 10f;
    private const float TwoPi = MathF.PI * 2f;
    private const string PreviewTextDefault = "A\r\nPreview";
    
    private static readonly Random Random = new();
    
    public readonly Vector2 SpiralSize = new(150);
    public Vector4 SpiralColor = new(1.0f, 0.25f, 1.0f, 0.5f);
    
    public Vector4 PreviewTextColor = Vector4.One;
    public string PreviewText = PreviewTextDefault;
    public int PreviewTextInterval = 1;

    private readonly Timer _currentDisplayPhraseTimer;
    private string _currentDisplayPhrase = PreviewTextDefault;
    private int _lastDisplayPhraseIndex;
    private string[] _hypnosisTextWordBank = ["A", "Preview"];
    
    public int SpiralSpeed = 50;
    
    private readonly string _spiralPath = Path.Combine(Plugin.PluginInterface.AssemblyLocation.Directory?.FullName!, SpiralName);
    private readonly Vector2[] _spiralUv =
    [
        new(0, 0),
        new(1, 0),
        new(1, 1),
        new(0, 1)
    ];
    
    private float _spiralRotation;

    public HypnosisViewUiController()
    {
        _currentDisplayPhraseTimer = new Timer(SpiralSpeed);
        _currentDisplayPhraseTimer.Interval = PreviewTextInterval;
        _currentDisplayPhraseTimer.Elapsed += ChangeDisplayPhrase;
        _currentDisplayPhraseTimer.AutoReset = true;
        _currentDisplayPhraseTimer.Start();
    }

    private void ChangeDisplayPhrase(object? sender, ElapsedEventArgs e)
    {
        if (_hypnosisTextWordBank.Length < 2)
            return;
        
        var next = Random.Next(0, _hypnosisTextWordBank.Length);
        while (next == _lastDisplayPhraseIndex)
            next = Random.Next(0, _hypnosisTextWordBank.Length);

        _lastDisplayPhraseIndex = next;
        _currentDisplayPhrase = _hypnosisTextWordBank[next];
    }

    public void RenderPreviewSpiral()
    {
        var spiral = Plugin.TextureProvider.GetFromFile(_spiralPath).GetWrapOrDefault();
        if (spiral is null)
            return;

        var speed = (SpiralSpeedMax - SpiralSpeedMin) * (SpiralSpeed * 0.01f) + SpiralSpeedMin;
        _spiralRotation += Plugin.Framework.UpdateDelta.Milliseconds * speed;
        _spiralRotation %= TwoPi;

        var drawList = ImGui.GetForegroundDrawList();
        var topLeft = ImGui.GetCursorScreenPos();
        var center = topLeft + SpiralSize * 0.5f;

        var corners = new[]
        {
            new Vector2(-SpiralSize.X, -SpiralSize.Y) * SpiralScalar,
            new Vector2(SpiralSize.X, -SpiralSize.Y) * SpiralScalar,
            new Vector2(SpiralSize.X, SpiralSize.Y) * SpiralScalar,
            new Vector2(-SpiralSize.X, SpiralSize.Y) * SpiralScalar
        };
        
        var cos = MathF.Cos(_spiralRotation);
        var sin = MathF.Sin(_spiralRotation);

        var rotated = new Vector2[4];
        for (var i = 0; i < 4; i++)
        {
            var x = corners[i].X;
            var y = corners[i].Y;

            rotated[i] = new Vector2(
                center.X + (cos * x - sin * y),
                center.Y + (sin * x + cos * y)
            );
        }
        
        var finalColor = ImGui.ColorConvertFloat4ToU32(SpiralColor);
        drawList.PushClipRect(topLeft, topLeft + SpiralSize , true);
        drawList.AddImageQuad(spiral.ImGuiHandle, rotated[0], rotated[1], rotated[2], rotated[3], _spiralUv[0], _spiralUv[1], _spiralUv[2], _spiralUv[3], finalColor);
        drawList.PopClipRect();
        
        SharedUserInterfaces.PushMediumFont();
        var size = ImGui.CalcTextSize(_currentDisplayPhrase);
        SharedUserInterfaces.PopMediumFont();
        
        var textColor = ImGui.ColorConvertFloat4ToU32(PreviewTextColor);
        drawList.AddText(SharedUserInterfaces.GetMediumFontPtr(), SharedUserInterfaces.MediumFontSize, center - size * 0.5f, textColor, _currentDisplayPhrase);
    }

    public void UpdateWordBank()
    {
        _hypnosisTextWordBank = PreviewText.Split(["\r\n", "\r", "\n"], StringSplitOptions.None);
    }
    
    private static readonly double[] PreviewSpeeds =
    [
        100, 200, 400, 800, 1200, 2000, 3000, 4000, 6000, 10000
    ];

    public void UpdatePreviewTestSpeed()
    {
        _currentDisplayPhraseTimer.Interval = PreviewSpeeds[PreviewTextInterval - 1];
    }
}