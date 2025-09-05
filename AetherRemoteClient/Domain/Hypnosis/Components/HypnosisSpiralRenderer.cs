using System;
using System.Numerics;
using System.Threading.Tasks;
using AetherRemoteClient.Utils;
using AetherRemoteCommon.Domain;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Textures.TextureWraps;

namespace AetherRemoteClient.Domain.Hypnosis.Components;

public class HypnosisSpiralRenderer : IDisposable
{
    // Const
    private const float TwoPi = MathF.PI * 2;
    private static readonly Vector2[] SpiralUv = [new(0, 0), new(1, 0), new(1, 1), new(0, 1)];
    
    // Defaults
    public const int DefaultSpiralArms = 1;
    public const int DefaultSpiralTurns = 8;
    public const int DefaultSpiralCurve = 5;
    public const int DefaultSpiralThickness = 10;
    public const int DefaultSpiralSpeed = 3;
    public const uint DefaultSpiralColor = 0x80FF40FF;
    public const HypnosisSpiralDirection DefaultSpiralDirection = HypnosisSpiralDirection.Clockwise;
    
    // Private values
    private IDalamudTextureWrap? _spiralTexture;
    private float _angle;
    
    // Configuration values
    private uint _color = DefaultSpiralColor;
    private HypnosisSpiralDirection _direction = DefaultSpiralDirection;
    private int _speed = DefaultSpiralSpeed;
    
    // Set properties
    public void SetColor(uint color) => _color = color;
    public void SetColor(Vector4 color) => _color = ImGui.ColorConvertFloat4ToU32(color);
    public void SetDirection(HypnosisSpiralDirection direction) => _direction = direction;
    public void SetSpeed(int speed) => _speed = speed;
    public async Task SetSpiral(int arms, int turns, float curve, float thickness)
    {
        var old = _spiralTexture;
        _spiralTexture = await HypnosisSpiralGenerator.Generate(arms, turns, curve, thickness, HypnosisSpiralDirection.Clockwise).ConfigureAwait(false);
        old?.Dispose();
    }
    
    // Render
    public void Render(ImDrawListPtr draw, Vector2 screenSize, Vector2 screenPosition)
    {
        if (_spiralTexture is null)
            return;

        _angle += Plugin.Framework.UpdateDelta.Milliseconds * 0.001f * _speed * (_direction is HypnosisSpiralDirection.Clockwise ? 1 : -1);
        _angle %= TwoPi;
        
        var cos = MathF.Cos(_angle);
        var sin = MathF.Sin(_angle);

        // Need to modify the size s
        var imageSpiralSize = screenSize;
        if (imageSpiralSize.X < imageSpiralSize.Y)
            imageSpiralSize.X = imageSpiralSize.Y;
        else 
            imageSpiralSize.Y = imageSpiralSize.X;
        
        var corners = new[]
        {
            new Vector2(-imageSpiralSize.X, -imageSpiralSize.Y),
            new Vector2(imageSpiralSize.X, -imageSpiralSize.Y),
            new Vector2(imageSpiralSize.X, imageSpiralSize.Y),
            new Vector2(-imageSpiralSize.X, imageSpiralSize.Y)
        };

        var center = screenPosition + screenSize * 0.5f;
        
        var rotated = new Vector2[4];
        for (var i = 0; i < 4; i++)
        {
            var x = corners[i].X;
            var y = corners[i].Y;
        
            rotated[i] = new Vector2(center.X + (cos * x - sin * y), center.Y + (sin * x + cos * y));
        }
        
        draw.AddImageQuad(_spiralTexture.Handle, rotated[0],  rotated[1], rotated[2], rotated[3], SpiralUv[0], SpiralUv[1], SpiralUv[2], SpiralUv[3], _color);
    }

    public void Dispose()
    {
        _spiralTexture?.Dispose();
        GC.SuppressFinalize(this);
    }
}