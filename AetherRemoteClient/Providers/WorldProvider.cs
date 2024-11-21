using System;
using System.Collections.Generic;
using Lumina.Excel;
using Lumina.Excel.Sheets;

namespace AetherRemoteClient.Providers;

/// <summary>
/// Provides a list all current available worlds in game
/// </summary>
public class WorldProvider
{
    /// <summary>
    /// List of all in game world names
    /// </summary>
    public readonly List<string> WorldNames;
    private readonly ExcelSheet<World> _worldSheet;

    /// <summary>
    /// <inheritdoc cref="WorldProvider"/>
    /// </summary>
    public WorldProvider()
    {
        _worldSheet = Plugin.DataManager.Excel.GetSheet<World>();
        
        var worldList = new List<string>();
        for (uint i = 0; i < _worldSheet.Count; i++)
        {
            var world = _worldSheet.GetRowOrDefault(i);
            if (world is null) continue;

            var name = world.Value.InternalName.ToString();
            if (ShouldIncludeWorld(name))
                worldList.Add(name);
        }

        worldList.Sort();
        WorldNames = [.. worldList];
    }

    /// <summary>
    /// Attempts to get a world name by world id
    /// </summary>
    /// <param name="worldId">The world id provided by <see cref="FFXIVClientStructs.FFXIV.Client.Game.Character.BattleChara.HomeWorld"/></param>
    /// <returns>World name, or null if not found</returns>
    public string? TryGetWorldById(ushort worldId)
    {
        try
        {
            var world = _worldSheet.GetRowOrDefault(worldId);
            if (world is not null)
                return world.Value.InternalName.ToString();
            
            Plugin.Log.Warning($"[WorldProvider] World {worldId} not found.");
            return null;

        }
        catch(Exception ex)
        {
            Plugin.Log.Warning($"[WorldProvider] Error during world lookup: {ex}");
            return null;
        }
    }

    /// <summary>
    /// Checks to see if the provided world name is a valid world name
    /// </summary>
    public bool IsValidWorld(string world) => WorldNames.Contains(world);

    /// <summary>
    /// Various worlds are developer, promotional, or incomplete and must be filtered out
    /// </summary>
    /// <param name="world"></param>
    /// <returns></returns>
    private static bool ShouldIncludeWorld(string world)
    {
        if (world == string.Empty) return false;
        if (char.IsUpper(world[0]) == false) return false;
        if (world == "Dev") return false;
        for(var i = world.Length - 1; i >= 0; i--)
            if (char.IsDigit(world[i])) return false;

        return true;
    }
}
