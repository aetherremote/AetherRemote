using System;
using System.Threading.Tasks;

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
                return await CreateCharacterConfiguration(characterName, characterWorld).ConfigureAwait(false);
            
            var id = reader.GetInt64(0);
            var secretId = reader.IsDBNull(1) ? (short?)null : reader.GetInt16(1);
            
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
    public async Task<bool> SetCharacterConfigurationSecret(long id, long secretId)
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
    private async Task<CharacterConfiguration?> CreateCharacterConfiguration(string characterName, string characterWorld)
    {
        try
        {
            await using var command = _database.CreateCommand();
            command.CommandText = "INSERT INTO Characters (Name, World, SecretId) VALUES (@Name, @World, null) RETURNING Id";
            command.Parameters.AddWithValue("@Name", characterName);
            command.Parameters.AddWithValue("@World", characterWorld);

            var id = Convert.ToInt64(await command.ExecuteScalarAsync().ConfigureAwait(false));
            return new CharacterConfiguration(id, characterName, characterWorld, null);
        }
        catch (Exception e)
        {
            Plugin.Log.Error($"[DatabaseInfrastructure.CreateCharacterConfiguration] {e}");
            return null;
        }
    }

    public class CharacterConfiguration(long id, string name, string world, long? secretId)
    {
        public readonly long Id = id;
        public readonly string Name = name;
        public readonly string World = world;
        public long? SecretId = secretId;
    }
}