using AetherRemoteClient.Domain;

namespace AetherRemoteClient.Services;

/// <summary>
///     Service for managing the current view
/// </summary>
public class ViewService
{
    /// <summary>
    ///     The current view to draw
    /// </summary>
    public View CurrentView { get; set; } = View.Login;
}