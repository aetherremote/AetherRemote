namespace AetherRemoteClient.Domain.Interfaces;

/// <summary>
///     An element that should be drawn every frame
/// </summary>
public interface IDrawable
{
    /// <summary>
    ///     Draw a view
    /// </summary>
    /// <returns>If the friend's list should be drawn too</returns>
    public bool Draw();
}