namespace AetherRemoteClient.Services;

/// <summary>
///     Simple service for interacting with the game's configuration values
/// </summary>
public class GameSettingsService
{
    /// <summary>
    ///     Sets the camera configuration value
    /// </summary>
    public static void SetMoveMode(uint moveMode)
    {
        Plugin.GameConfig.UiControl.Set("MoveMode", moveMode);
    }

    /// <summary>
    ///     Gets the control type for this input
    /// </summary>
    public static uint? TryGetMoveMode()
    {
        return Plugin.GameConfig.UiControl.TryGet("MoveMode", out uint mode) ? mode : null;
    }
}