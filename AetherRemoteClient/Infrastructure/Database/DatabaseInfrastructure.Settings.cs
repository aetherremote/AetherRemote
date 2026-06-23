using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AetherRemoteClient.Domain.Enums;

namespace AetherRemoteClient.Infrastructure.Database;

public partial class DatabaseInfrastructure
{
    /// <summary>
    ///     Loads all the settings for a secret id from the table in their raw data string
    /// </summary>
    public async Task<Dictionary<Setting, string>?> GetSettingsForSecretId(long secretId)
    {
        try
        {
            await using var command = _database.CreateCommand();
            command.CommandText = "SELECT SettingId, Value FROM Settings WHERE SecretId = @SecretId";
            command.Parameters.AddWithValue("@SecretId", secretId);

            var results = new Dictionary<Setting, string>();
            
            await using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync().ConfigureAwait(false))
            {
                var setting = (Setting)reader.GetInt16(0);
                var value = reader.GetString(1);
                results.Add(setting, value);
            }
            
            return results;
        }
        catch (Exception e)
        {
            Plugin.Log.Error($"[DatabaseInfrastructure.GetSettingsForSecretId] {e}");
            return null;
        }
    }
    
    /// <summary>
    ///     Sets a note for a friend code
    /// </summary>
    public async Task<bool> SetSetting(long secretId, Setting setting, string value)
    {
        try
        {
            await using var command = _database.CreateCommand();
            command.CommandText = "INSERT INTO Settings VALUES (@SecretId, @SettingId, @Value) ON CONFLICT (SecretId, SettingId) DO UPDATE SET Value = @Value";
            command.Parameters.AddWithValue("@SecretId", secretId);
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