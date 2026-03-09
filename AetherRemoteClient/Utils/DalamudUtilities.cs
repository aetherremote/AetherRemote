using System;
using System.Threading.Tasks;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Game.ClientState.Objects.Types;

namespace AetherRemoteClient.Utils;

public static class DalamudUtilities
{
    /// <summary>
    ///     Runs a function on the XIV Framework
    /// </summary>
    /// <remarks>Await should never be called inside the function</remarks>
    public static async Task<T> RunOnFramework<T>(Func<T> func)
    {
        if (Plugin.Framework.IsInFrameworkUpdateThread)
            return func();
        
        return await Plugin.Framework.RunOnFrameworkThread(func).ConfigureAwait(false);
    }

    /// <summary>
    ///     Runs a function on the XIV Framework
    /// </summary>
    /// <remarks>Await should never be called inside the function</remarks>
    public static async Task RunOnFramework(Action func)
    {
        if (Plugin.Framework.IsInFrameworkUpdateThread)
            func();
        else
            await Plugin.Framework.RunOnFrameworkThread(func).ConfigureAwait(false);
    }

    /// <summary>
    ///     Attempts to get the local player
    /// </summary>
    /// <returns></returns>
    public static async Task<IPlayerCharacter?> TryGetLocalPlayer()
    {
        try
        {
            return await RunOnFramework(() => Plugin.ObjectTable.LocalPlayer).ConfigureAwait(false);
        }
        catch (Exception e)
        {
            Plugin.Log.Error($"[DalamudUtilities.TryGetLocalPlayer] {e}");
            return null;
        }
    }
    
    /// <summary>
    ///     Attempts to get a player's game object by name and home world
    /// </summary>
    public static async Task<IPlayerCharacter?> TryGetPlayerFromObjectTable(string characterName, string characterWorld)
    {
        try
        {
            // Get a game object for target player in object table
            return await RunOnFramework(() =>
            {
                // Iterate through the object table
                for (ushort i = 0; i < Plugin.ObjectTable.Length; i++)
                {
                    // Skip if the object is not a player
                    if (Plugin.ObjectTable[i] is not IPlayerCharacter playerCharacter)
                        continue;
                    
                    // If the name and world match, return
                    if (playerCharacter.Name.ToString() == characterName && playerCharacter.HomeWorld.Value.Name.ToString() == characterWorld)
                        return playerCharacter;
                }

                // No objects found that matched
                return null;
            }).ConfigureAwait(false);
        }
        catch (Exception e)
        {
            Plugin.Log.Error($"[DalamudUtilities.TryGetPlayerFromObjectTable] {e}");
            return null;
        }
    }
}