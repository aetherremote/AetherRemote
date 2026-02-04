using System.Numerics;

namespace AetherRemoteClient.Style;

/// <summary>
///     Container for various component sizes
/// </summary>
public static class AetherRemoteDimensions
{
    public const int SendCommandButtonHeight = 40;
    
    public static readonly Vector2 NavBar = new(180, 0);
    public static readonly Vector2 IconButton = new(24, 24);

    public static readonly Vector2 Tooltip = new(400, 0);
}