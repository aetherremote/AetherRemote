using System.Threading.Tasks;

namespace AetherRemoteClient.Services;

/// <summary>
///     Simple service for interacting with the game's configuration values
/// </summary>
public class GameSettingsService
{
    /// <summary>
    ///     Sets the camera configuration value
    /// </summary>
    public static async Task SetMoveMode(uint moveMode)
    {
        await Plugin.RunOnFramework(() => Plugin.GameConfig.UiControl.Set("MoveMode", moveMode)).ConfigureAwait(false);
    }

    /// <summary>
    ///     Gets the control type for this input
    /// </summary>
    public static async Task<uint?> TryGetMoveMode()
    {
        return await Plugin.RunOnFramework<uint?>(() => Plugin.GameConfig.UiControl.TryGetUInt("MoveMode", out var moveMode) ? moveMode : null).ConfigureAwait(false);
    }
}