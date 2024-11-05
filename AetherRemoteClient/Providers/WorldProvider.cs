using Lumina.Excel;
using Lumina.Excel.GeneratedSheets;
using System;
using System.Collections.Generic;

namespace AetherRemoteClient.Providers;

public class WorldProvider
{
    public readonly List<string> WorldNames;
    private readonly ExcelSheet<World>? _worldSheet;

    public WorldProvider()
    {
        _worldSheet = Plugin.DataManager.Excel.GetSheet<World>();
        if (_worldSheet is null)
        {
            Plugin.Log.Warning("Unable to retrieve World excel sheet.");
            WorldNames = [];
            return;
        }

        var worldList = new List<string>();
        for (uint i = 0; i < _worldSheet.RowCount; i++)
        {
            var row = _worldSheet.GetRow(i);
            var name = row?.Name.RawString;
            if (name is null) continue;

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
            return _worldSheet?.GetRow(worldId)?.InternalName?.ToString();
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
