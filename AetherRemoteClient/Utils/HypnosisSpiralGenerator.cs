using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Threading.Tasks;
using AetherRemoteCommon.Domain;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Textures.TextureWraps;
using SkiaSharp;

namespace AetherRemoteClient.Utils;

/// <summary>
///     Assists in the generation of spirals that fit the local player's monitor specs
/// </summary>
public static class HypnosisSpiralGenerator
{
    // Pre-compute so we don't need to do any division later
    private const float RadToDegree = 180f / MathF.PI;

    // We need to divide by 20 so that our range defined from 1 to 10 stays within (0, 0.5]
    private const float SpiralThicknessRatio = 20f;
    
    // Always render spirals as white so they can be colored later
    private static readonly SKColor White = new(255, 255, 255, 255);

    /// <summary>
    ///     Generate a spiral texture for use with ImGui and Dalamud
    /// </summary>
    /// <param name="arms">How many arms should the spiral contain</param>
    /// <param name="turns">How many times the spiral circles itself before reaching the center</param>
    /// <param name="curve">How aggressive should the curving be</param>
    /// <param name="thickness">How thick are the spiral arms</param>
    /// <param name="direction">Which direction the spiral wraps</param>
    /// <returns>The spiral texture</returns>
    public static async Task<IDalamudTextureWrap?> Generate(int arms, int turns, float curve, float thickness, HypnosisSpiralDirection direction)
    {
        // Determine direction
        var counterClockwise = direction is HypnosisSpiralDirection.CounterClockwise;
        
        // Generate the points
        var path = new SKPath();
        var size = CalculateMinimumSpiralImageSize(ImGui.GetIO().DisplaySize);
        var width = (thickness / SpiralThicknessRatio) * (1f / arms) * (arms % 2 == 0 ? -1 : 1);
        for (var arm = 0; arm < arms; arm++)
        {
            path.MoveTo(size, size);
            var offset = (float)arm / arms;
            var outside = CalculateSpiralArmPoints(turns, offset, curve, size, counterClockwise);
            var inside = CalculateSpiralArmPoints(turns, offset + width, curve, size, counterClockwise);
            AddSpiralArmPointsToPath(path, outside, false);
            AddArcToConnectSpiralArmPointsToPath(path, outside[^1], inside[^1], size, false);
            AddSpiralArmPointsToPath(path, inside, true);
        }

        // Create the surface
        using var paint = new SKPaint();
        paint.Style = SKPaintStyle.Fill;
        paint.Color = White;
        paint.IsAntialias = true;

        var info = new SKImageInfo
        {
            Width = (int)size * 2,
            Height = (int)size * 2,
            ColorType = SKColorType.Rgba8888,
            AlphaType = SKAlphaType.Premul
        };
        
        // Draw to the surface
        using var bitmap = new SKBitmap(info);
        using var canvas = new SKCanvas(bitmap);
        canvas.Clear(SKColors.Transparent);
        canvas.DrawPath(path, paint);

        // Convert to a stream
        using var image = SKImage.FromBitmap(bitmap);
        using var data = image.Encode(SKEncodedImageFormat.Png, 100);
        using var stream = new MemoryStream();
        data.SaveTo(stream);
        stream.Position = 0;

        try
        {
            // Return the stream in a format dalamud can use
            return await Plugin.TextureProvider.CreateFromImageAsync(stream);
        }
        catch (Exception e)
        {
            Plugin.Log.Info(e.ToString());
            return null;
        }
    }

    /// <summary>
    ///     Calculates all the points to be used for rendering along a spiral's arm
    /// </summary>
    private static List<SKPoint> CalculateSpiralArmPoints(float turns, float offset, float curve, float size, bool counterClockwise)
    {
        var result = new List<SKPoint>();
        var steps = 180 * turns;
        var increment = 1f / steps;
        for (var step = 0f; step <= 1; step += increment)
        {
            var smoothing = MathF.Pow(step, curve);
            var radius = size * smoothing;
            var rotation = counterClockwise
                ? float.Tau - (step * turns + offset) * float.Tau
                : (step * turns + offset) * float.Tau;
            
            result.Add(new SKPoint(size + radius * MathF.Cos(rotation), size + radius * MathF.Sin(rotation)));
        }

        return result;
    }

    /// <summary>
    ///     Adds all the points previously created with <see cref="CalculateSpiralArmPoints"/> to the supplied path
    /// </summary>
    private static void AddSpiralArmPointsToPath(SKPath path, List<SKPoint> points, bool counterClockwise)
    {
        if (counterClockwise)
            points.Reverse();
        
        foreach (var point in points)
            path.LineTo(point);
    }

    /// <summary>
    ///     Connects the end points of two paths that make up a full arm with an arc
    /// </summary>
    private static void AddArcToConnectSpiralArmPointsToPath(SKPath path, SKPoint finalOutsidePoint, SKPoint finalInsidePoint, float size, bool counterClockwise)
    {
        var startAngle = MathF.Atan2(finalOutsidePoint.Y - size, finalOutsidePoint.X - size) * RadToDegree;
        var endAngle = MathF.Atan2(finalInsidePoint.Y - size, finalInsidePoint.X - size) * RadToDegree;
        var sweepAngle = endAngle - startAngle;
        if (counterClockwise)
            sweepAngle *= -1f;
        
        var oval = new SKRect(0, 0, size * 2, size * 2);
        path.AddArc(oval, startAngle, sweepAngle);
    }
    
    /// <summary>
    ///     Calculate the minimum size needed to generate the spiral at to fill the screen
    /// </summary>
    private static float CalculateMinimumSpiralImageSize(Vector2 screenSize)
    {
        // Half of the size
        var half = screenSize * 0.5f;
        
        // The hypotenuse will help make sure that the spiral will feel all corners of the screen
        var hypotenuse = MathF.Sqrt(half.X * half.X + half.Y * half.Y);
        
        // Round up and double to provide the full width of the resulting spiral
        return MathF.Ceiling(hypotenuse * 1.3f); // Buffing size slightly to allow for better resolution
    }
}