using Lumina.Excel;
using Lumina.Excel.GeneratedSheets;
using System;

namespace AetherRemoteClient.Providers;

public class WorldProvider
{
    private readonly ExcelSheet<World>? worldSheet;

    public WorldProvider()
    {
        worldSheet = Plugin.DataManager.Excel.GetSheet<World>();
        if (worldSheet == null)
            Plugin.Log.Warning("Unable to retrieve World excel sheet.");
    }

    public string? TryGetWorld(ushort worldId)
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
}
