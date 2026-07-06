using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AetherRemoteClient.Domain;

namespace AetherRemoteClient.Infrastructure.Database;

public partial class DatabaseInfrastructure
{
    /// <summary>
    ///     Attempts to load a character configuration from the database. If one doesn't exist, it will be created and returned
    /// </summary>
    public async Task<CharacterConfiguration?> GetCharacterConfiguration(string characterName, string characterWorld)
    {
        try
        {
            await using var command = _database.CreateCommand();
            command.CommandText = "SELECT Id, SecretId FROM Characters WHERE Name = @Name AND World = @World LIMIT 1";
            command.Parameters.AddWithValue("@Name", characterName);
            command.Parameters.AddWithValue("@World", characterWorld);
            
            await using var reader = await command.ExecuteReaderAsync();

            // If we didn't find anything attempt to create a new configuration for the local player since they will need it anyway
            if (await reader.ReadAsync().ConfigureAwait(false) is false)
                return await CreateCharacterConfiguration(characterName, characterWorld, null).ConfigureAwait(false);
            
            var id = reader.GetInt64(0);
            var secretId = reader.IsDBNull(1) ? (long?)null : reader.GetInt16(1);
            
            return new CharacterConfiguration(id, characterName, characterWorld, secretId);
        }
        catch (Exception e)
        {
            Plugin.Log.Error($"[DatabaseInfrastructure.GetCharacterConfiguration] {e}");
            return null;
        }
    }

    /// <summary>
    ///     Updates a known character configuration with the new secret id to use
    /// </summary>
    public async Task<bool> SetCharacterSecretId(long id, long secretId)
    {
        try
        {
            await using var command = _database.CreateCommand();
            command.CommandText = "UPDATE Characters SET SecretId = @SecretId WHERE Id = @Id";
            command.Parameters.AddWithValue("@SecretId", secretId);
            command.Parameters.AddWithValue("@Id", id);

            return await command.ExecuteNonQueryAsync().ConfigureAwait(false) is 1;
        }
        catch (Exception e)
        {
            Plugin.Log.Error($"[DatabaseInfrastructure.SetCharacterConfigurationSecret] {e}");
            return false;
        }
    }

    /// <summary>
    ///     Creates a new character configuration
    /// </summary>
    public async Task<CharacterConfiguration?> CreateCharacterConfiguration(string characterName, string characterWorld, long? secretId)
    {
        try
        {
            await using var command = _database.CreateCommand();
            command.CommandText = "INSERT INTO Characters (Name, World, SecretId) VALUES (@Name, @World, @SecretId) RETURNING Id";
            command.Parameters.AddWithValue("@Name", characterName);
            command.Parameters.AddWithValue("@World", characterWorld);
            command.Parameters.AddWithValue("@SecretId", (object?)secretId ?? DBNull.Value);

            var id = Convert.ToInt64(await command.ExecuteScalarAsync().ConfigureAwait(false));
            return new CharacterConfiguration(id, characterName, characterWorld, secretId);
        }
        catch (Exception e)
        {
            Plugin.Log.Error($"[DatabaseInfrastructure.CreateCharacterConfiguration] {e}");
            return null;
        }
    }

    /// <summary>
    ///     Gets all the character configurations that have a secret id set
    /// </summary>
    public async Task<Dictionary<long, CharacterConfiguration>?> GetCharacterConfigurations()
    {
        try
        {
            await using var command = _database.CreateCommand();
            command.CommandText = "SELECT Id, Name, World, SecretId FROM Characters WHERE SecretId IS NOT NULL";
            
            var results = new Dictionary<long, CharacterConfiguration>();
            
            await using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync().ConfigureAwait(false))
            {
                var id = reader.GetInt64(0);
                var name = reader.GetString(1);
                var world = reader.GetString(2);
                var secretId = reader.GetInt64(3);
                
                results.Add(id, new CharacterConfiguration(id, name, world, secretId));
            }
            
            return results;
        }
        catch (Exception e)
        {
            Plugin.Log.Error($"[DatabaseInfrastructure.CountOccurrences] {e}");
            return null;
        }
    }
}