using System.Threading.Tasks;
using AetherRemoteClient.Utils;

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
        await DalamudUtilities.RunOnFramework(() => Plugin.GameConfig.UiControl.Set("MoveMode", moveMode)).ConfigureAwait(false);
    }

    /// <summary>
    ///     Gets the control type for this input
    /// </summary>
    public static async Task<uint?> TryGetMoveMode()
    {
        return await DalamudUtilities.RunOnFramework<uint?>(() => Plugin.GameConfig.UiControl.TryGetUInt("MoveMode", out var moveMode) ? moveMode : null).ConfigureAwait(false);
    }
}