using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AetherRemoteClient.Domain.Enums;

namespace AetherRemoteClient.Infrastructure.Database;

public partial class DatabaseInfrastructure
{   
    /// <summary>
    ///     Loads all the settings for the plugin
    /// </summary>
    public async Task<Dictionary<Settings, string>?> GetSettings()
    {
        try
        {
            await using var command = _database.CreateCommand();
            command.CommandText = "SELECT SettingId, Value FROM Settings";
            
            var results = new Dictionary<Settings, string>();
            
            await using var reader = await command.ExecuteReaderAsync().ConfigureAwait(false);
            while (await reader.ReadAsync().ConfigureAwait(false))
            {
                var setting = (Settings)reader.GetInt16(0);
                var value = reader.GetString(1);
                results.Add(setting, value);
            }
            
            return results;
        }
        catch (Exception e)
        {
            Plugin.Log.Error($"[DatabaseInfrastructure.GetSettings] {e}");
            return null;
        }
    }

    /// <summary>
    ///     Sets a specific <see cref="Settings"/>'s value
    /// </summary>
    public async Task<bool> SetSetting(Settings setting, string value)
    {
        try
        {
            await using var command = _database.CreateCommand();
            command.CommandText = "INSERT INTO Settings VALUES (@SettingId, @Value) ON CONFLICT (SettingId) DO UPDATE SET Value = @Value";
            command.Parameters.AddWithValue("@SettingId", setting);
            command.Parameters.AddWithValue("@Value", value);
            return await command.ExecuteNonQueryAsync().ConfigureAwait(false) is 1;
        }
        catch (Exception e)
        {
            Plugin.Log.Error($"[DatabaseInfrastructure.SetSetting] {e}");
            return false;
        }
    }
}