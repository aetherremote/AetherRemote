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
    public View CurrentView { get; private set; } = View.Login;

    /// <summary>
    ///     Set the current view
    /// </summary>
    public void Navigate(View view)
    {
        if (CurrentView == view)
            return;
        
        CurrentView = view;
    }

    /// <summary>
    ///     Set the current view to whatever the 'Home' view should be
    /// </summary>
    public void Home() => Navigate(View.Home);

    /// <summary>
    ///     Resets the view to login if it is not on the settings page or the login page
    /// </summary>
    public void ResetView()
    {
        if (CurrentView is View.Settings or View.Login)
            return;
        
        Navigate(View.Login);
    }
}