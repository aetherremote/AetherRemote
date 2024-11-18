using Dalamud.Game.ClientState.Objects.Types;

namespace AetherRemoteClient.Uncategorized;

/// <summary>
/// Provides various methods to retrieve objects from the game
/// </summary>
public static class GameObjectManager
{
    /// <summary>
    /// Checks to see if a local player exists
    /// </summary>
    /// <returns>True if local player is found, False otherwise</returns>
    public static bool LocalPlayerExists() => Plugin.ClientState.LocalPlayer is not null;
    
    /// <summary>
    /// Attempts to get the local player's name
    /// </summary>
    /// <returns>The player name if local player is found, otherwise null</returns>
    public static string? GetLocalPlayerName() => Plugin.ClientState.LocalPlayer?.Name.ToString();
    
    /// <summary>
    /// Attempts to get the local player's game object
    /// </summary>
    /// <returns>The game object if local player is found, otherwise null</returns>
    public static IGameObject? GetLocalPlayer() => Plugin.ClientState.LocalPlayer;
    
    /// <summary>
    /// Attempts to get the target player's object table index
    /// </summary>
    /// <returns>The object table index if a valid target, otherwise null</returns>
    public static ushort? GetTargetPlayerObjectIndex() => Plugin.TargetManager.Target?.ObjectIndex;

    /// <summary>
    /// Attempts to get the target player
    /// </summary>
    /// <returns>The target player, otherwise null</returns>
    public static IGameObject? GetTargetPlayer() => Plugin.TargetManager.Target;

    /// <summary>
    /// Gets the length of the object table
    /// </summary>
    /// <returns>The length of the object table</returns>
    public static int GetObjectTableLength() => Plugin.ObjectTable.Length;
    
    /// <summary>
    /// Attempts to get an item from the object table
    /// </summary>
    /// <param name="index">Index of the item</param>
    /// <returns>The item if valid, otherwise null</returns>
    public static IGameObject? GetObjectTableItem(int index) => Plugin.ObjectTable[index];
}
