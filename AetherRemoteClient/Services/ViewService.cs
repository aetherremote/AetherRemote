using AetherRemoteClient.Domain;
using Serilog;

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

    /// <summary>
    ///     Set the current view to whatever the 'Home' view should be
    /// </summary>
    public void Home()
    {
        CurrentView = View.Home;
    }

    /// <summary>
    ///     Resets the view to login if it is not on the settings page or the login page
    /// </summary>
    public void ResetView()
    {
        if (CurrentView is View.Settings or View.Login)
            return;
        
        CurrentView = View.Login;
    }
}