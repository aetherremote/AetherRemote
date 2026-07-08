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
    public async Task<Dictionary<SecretSetting, string>?> GetSecretSettings(long secretId)
    {
        try
        {
            await using var command = _database.CreateCommand();
            command.CommandText = "SELECT SettingId, Value FROM SecretSettings WHERE SecretId = @SecretId";
            command.Parameters.AddWithValue("@SecretId", secretId);

            var results = new Dictionary<SecretSetting, string>();
            
            await using var reader = await command.ExecuteReaderAsync().ConfigureAwait(false);
            while (await reader.ReadAsync().ConfigureAwait(false))
            {
                var setting = (SecretSetting)reader.GetInt16(0);
                var value = reader.GetString(1);
                results.Add(setting, value);
            }
            
            return results;
        }
        catch (Exception e)
        {
            Plugin.Log.Error($"[DatabaseInfrastructure.GetSecretSettings] {e}");
            return null;
        }
    }
    
    /// <summary>
    ///     Sets a specified setting for the provided secret id
    /// </summary>
    public async Task<bool> SetSecretSetting(long secretId, SecretSetting secretSetting, string value)
    {
        try
        {
            await using var command = _database.CreateCommand();
            command.CommandText = "INSERT INTO SecretSettings VALUES (@SecretId, @SettingId, @Value) ON CONFLICT (SecretId, SettingId) DO UPDATE SET Value = @Value";
            command.Parameters.AddWithValue("@SecretId", secretId);
            command.Parameters.AddWithValue("@SettingId", secretSetting);
            command.Parameters.AddWithValue("@Value", value);

            return await command.ExecuteNonQueryAsync().ConfigureAwait(false) is 1;
        }
        catch (Exception e)
        {
            Plugin.Log.Error($"[DatabaseInfrastructure.SetSetting] {e}");
            return false;
        }
    }
    
    /// <inheritdoc cref="SetSecretSetting(long, SecretSetting, string)"/>
    public async Task<bool> SetSecretSetting(long secretId, SecretSetting secretSetting, bool value) => await SetSecretSetting(secretId, secretSetting, value.ToString());
}