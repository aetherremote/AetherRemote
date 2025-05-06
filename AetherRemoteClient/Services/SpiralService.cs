using System;
using System.IO;
using System.Numerics;
using System.Timers;
using AetherRemoteClient.Utils;
using AetherRemoteCommon.Domain;
using AetherRemoteCommon.Domain.Enums;
using ImGuiNET;

namespace AetherRemoteClient.Services;

/// <summary>
///     Manages functionality to render spirals to the screen
/// </summary>
public class SpiralService : IDisposable
{
    // Util
    private const float TwoPi = MathF.PI * 2f;
    private static readonly Random Random = new();

    // Spiral Location
    private const string SpiralName = "spiral.png";
    private readonly string _spiralPath =
        Path.Combine(Plugin.PluginInterface.AssemblyLocation.Directory?.FullName!, SpiralName);

    // Speed
    private const float SpiralSpeedMax = 0.003f;
    private const float SpiralSpeedMin = 0;

    // Corners
    private readonly Vector2[] _spiralUv = [new(0, 0), new(1, 0), new(1, 1), new(0, 1)];

    // Text
    private readonly Timer _currentDisplayPhraseTimer;
    private string _currentDisplayPhrase = string.Empty;
    private int _lastDisplayPhraseIndex;

    // Current spiral info
    private SpiralInfo? _currentSpiral;
    private string _currentSender = string.Empty;

    // Spiral functional info
    private float _currentSpiralRotation;
    private readonly Timer _spiralExpirationTimer;

    public bool IsBeingHypnotized => _currentSpiral is not null;
    public string Sender => _currentSender;

    /// <summary>
    ///     <inheritdoc cref="SpiralService"/>
    /// </summary>
    public SpiralService()
    {
        _spiralExpirationTimer = new Timer(int.MaxValue) { AutoReset = false };
        _spiralExpirationTimer.Elapsed += SpiralExpirationTimerOnElapsed;

        _currentDisplayPhraseTimer = new Timer(int.MaxValue) { AutoReset = true };
        _currentDisplayPhraseTimer.Elapsed += ChangeDisplayPhrase;
        
        Plugin.PluginInterface.UiBuilder.Draw += Update;
    }

    /// <summary>
    ///     Begins drawling a spiral to the screen
    /// </summary>
    public void StartSpiral(SpiralInfo spiralInfo, string sender)
    {
        _currentSpiral = spiralInfo;
        _currentSender = sender;

        _currentDisplayPhraseTimer.Interval = spiralInfo.TextSpeed;
        _currentDisplayPhraseTimer.Start();

        if (spiralInfo.Duration is 0)
            return;

        _spiralExpirationTimer.Interval = spiralInfo.Duration * 60 * 1000;
        _spiralExpirationTimer.Start();
    }

    /// <summary>
    ///     Stops drawing a spiral to the screen
    /// </summary>
    public void StopCurrentSpiral()
    {
        _currentSpiral = null;
        _currentSender = string.Empty;
        _spiralExpirationTimer.Stop();
    }

    /// <summary>
    ///     To be called once a frame to render any current spirals
    /// </summary>
    private void Update()
    {
        if (_currentSpiral is null)
            return;

        var spiral = Plugin.TextureProvider.GetFromFile(_spiralPath).GetWrapOrDefault();
        if (spiral is null)
            return;

        var speed = (SpiralSpeedMax - SpiralSpeedMin) * (_currentSpiral.Speed * 0.01f) + SpiralSpeedMin;
        _currentSpiralRotation += Plugin.Framework.UpdateDelta.Milliseconds * speed;
        _currentSpiralRotation %= TwoPi;

        var drawList = ImGui.GetForegroundDrawList();
        var center = ImGui.GetIO().DisplaySize * 0.5f;

        var cos = MathF.Cos(_currentSpiralRotation);
        var sin = MathF.Sin(_currentSpiralRotation);

        var corners = new[]
        {
            new Vector2(-spiral.Width, -spiral.Height) * 0.4f,
            new Vector2(spiral.Width, -spiral.Height) * 0.4f,
            new Vector2(spiral.Width, spiral.Height) * 0.4f,
            new Vector2(-spiral.Width, spiral.Height) * 0.4f
        };

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

        var finalColor = ImGui.ColorConvertFloat4ToU32(_currentSpiral.Color);
        drawList.AddImageQuad(spiral.ImGuiHandle, rotated[0], rotated[1], rotated[2], rotated[3], _spiralUv[0],
            _spiralUv[1], _spiralUv[2], _spiralUv[3], finalColor);

        SharedUserInterfaces.PushMassiveFont();
        var size = ImGui.CalcTextSize(_currentDisplayPhrase);
        SharedUserInterfaces.PopMassiveFont();

        var textColor = ImGui.ColorConvertFloat4ToU32(_currentSpiral.TextColor);
        drawList.AddText(SharedUserInterfaces.MassiveFontPtr, SharedUserInterfaces.MassiveFontSize,
            center - size * 0.5f,
            textColor, _currentDisplayPhrase);
    }

    private void SpiralExpirationTimerOnElapsed(object? sender, ElapsedEventArgs e)
    {
        _currentSpiral = null;
    }

    private void ChangeDisplayPhrase(object? sender, ElapsedEventArgs e)
    {
        if (_currentSpiral is null)
            return;

        switch (_currentSpiral.WordBank.Length)
        {
            case 0:
                _currentDisplayPhrase = string.Empty;
                return;
            case 1:
                _currentDisplayPhrase = _currentSpiral.WordBank[0];
                return;
        }

        if (_currentSpiral.TextMode is SpiralTextMode.Random)
        {
            var next = Random.Next(0, _currentSpiral.WordBank.Length);
            while (next == _lastDisplayPhraseIndex)
                next = Random.Next(0, _currentSpiral.WordBank.Length);

            _lastDisplayPhraseIndex = next;
            _currentDisplayPhrase = _currentSpiral.WordBank[next];
        }
        else
        {
            _lastDisplayPhraseIndex++;
            _lastDisplayPhraseIndex %= _currentSpiral.WordBank.Length;
            _currentDisplayPhrase = _currentSpiral.WordBank[_lastDisplayPhraseIndex];
        }
    }

    public void Dispose()
    {
        _currentDisplayPhraseTimer.Elapsed -= SpiralExpirationTimerOnElapsed;
        _spiralExpirationTimer.Elapsed -= SpiralExpirationTimerOnElapsed;
        
        Plugin.PluginInterface.UiBuilder.Draw -= Update;
        
        GC.SuppressFinalize(this);
    }
}