namespace AetherRemoteClient.Services;

/// <summary>
///     Simple service for interacting with the game's configuration values
/// </summary>
public class GameSettingsService
{
    /// <summary>
    ///     Sets the camera configuration value to use the standard camera
    /// </summary>
    public static void SetToStandard()
    {
        Plugin.GameConfig.UiControl.Set("MoveMode", 0);
    }
    
    /// <summary>
    ///     Sets the camera configuration value ot use the legacy camera
    /// </summary>
    /// <param name="disableCameraPivot">A sub option to disable camera turning while mosting</param>
    public static void SetToLegacy(bool disableCameraPivot)
    {
        Plugin.GameConfig.UiControl.Set("MoveMode", 1);
        Plugin.GameConfig.UiControl.Set("LegacyCameraCorrectionFix", disableCameraPivot);
    }
}