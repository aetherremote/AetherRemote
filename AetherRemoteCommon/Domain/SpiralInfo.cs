using System.Numerics;
using AetherRemoteCommon.Domain.Enums;
using MessagePack;

namespace AetherRemoteCommon.Domain;

/// <summary>
///     Represents all the data required to render a spiral
/// </summary>
[MessagePackObject(keyAsPropertyName: true)]
public record SpiralInfo
{
    /// <summary>
    ///     How long the spiral lasts for in minutes. Set to 0 for indefinitely
    /// </summary>
    public int Duration { get; set; }
    
    /// <summary>
    ///     How fast the spiral spins. Higher values spin faster
    /// </summary>
    public float Speed  { get; set; }
    
    /// <summary>
    ///     How fast the spiral text changes. Higher values change text more frequent
    /// </summary>
    public float TextSpeed  { get; set; }
    
    /// <summary>
    ///     The color of the spiral
    /// </summary>
    public SerializableVector4 Color { get; set; } = Vector4.Zero;
    
    /// <summary>
    ///     The color of the text
    /// </summary>
    public SerializableVector4 TextColor { get; set; } = Vector4.Zero;
    
    /// <summary>
    ///     How the text should cycle, more detail in <see cref="SpiralTextMode"/>
    /// </summary>
    public SpiralTextMode TextMode { get; set; } = SpiralTextMode.Random;
    
    /// <summary>
    ///     The words that display in the spiral
    /// </summary>
    public string[] WordBank { get; set; } = [];
}