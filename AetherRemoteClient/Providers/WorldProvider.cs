using Lumina.Excel;
using Lumina.Excel.GeneratedSheets;
using System;
using System.Collections.Generic;

namespace AetherRemoteClient.Providers;

public class WorldProvider
{
    public readonly List<string> WorldNames;
    private readonly ExcelSheet<World>? worldSheet;

    public WorldProvider()
    {
        worldSheet = Plugin.DataManager.Excel.GetSheet<World>();
        if (worldSheet == null)
        {
            Plugin.Log.Warning("Unable to retrieve World excel sheet.");
            WorldNames = [];
            return;
        }

        var worldList = new List<string>();
        for (uint i = 0; i < worldSheet.RowCount; i++)
        {
            var row = worldSheet.GetRow(i);
            if (row == null) continue;

            var name = row.Name.RawString;
            if (name == null) continue;

            if (ShouldIncludeWorld(name))
                worldList.Add(name);
        }

        worldList.Sort();
        WorldNames = [.. worldList];
    }

    public string? TryGetWorldById(ushort worldId)
    {
        try
        {
            return worldSheet?.GetRow(worldId)?.InternalName?.ToString();
        }
        catch(Exception ex)
        {
            Plugin.Log.Warning($"Exception during world lookup: {ex}");
            return null;
        }
    }

    public bool IsValidWorld(string world) => WorldNames.Contains(world);

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
