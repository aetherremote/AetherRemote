using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AetherRemoteClient.Infrastructure.Database;

public partial class DatabaseInfrastructure
{
    /// <summary>
    ///     Loads all the settings for a secret id from the table
    /// </summary>
    public async Task<Dictionary<string, string>?> GetSettingsForSecretId(long secretId)
    {
        try
        {
            await using var command = _database.CreateCommand();
            command.CommandText = "SELECT Name, Value FROM Settings WHERE SecretId = @SecretId";
            command.Parameters.AddWithValue("@SecretId", secretId);

            var results = new Dictionary<string, string>();
            
            await using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync().ConfigureAwait(false))
            {
                var name = reader.GetString(0);
                var value = reader.GetString(1);
                
                results.Add(name, value);
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
    public async Task<bool> SetSetting(long secretId, string name, string value)
    {
        try
        {
            await using var command = _database.CreateCommand();
            command.CommandText = "INSERT INTO Settings VALUES (@SecretId, @Name, @Value) ON CONFLICT (SecretId, Name) DO UPDATE SET Value = @Value";
            command.Parameters.AddWithValue("@SecretId", secretId);
            command.Parameters.AddWithValue("@Name", name);
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