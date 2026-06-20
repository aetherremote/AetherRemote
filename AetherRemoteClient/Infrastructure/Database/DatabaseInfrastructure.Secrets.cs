using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;

namespace AetherRemoteClient.Infrastructure.Database;

public partial class DatabaseInfrastructure
{
    /// <summary>
    ///     Loads all the secrets from the table
    /// </summary>
    public async Task<Dictionary<long, Secret>?> GetSecrets()
    {
        try
        {
            await using var command = _database.CreateCommand();
            command.CommandText = "SELECT Id, Name, Secret, CreatedAt FROM Secrets";

            var results = new Dictionary<long, Secret>();
            
            await using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync().ConfigureAwait(false))
            {
                var id = reader.GetInt64(0);
                var name = reader.GetString(1);
                var secret = reader.GetString(2);
                var createdAt = reader.GetString(3);
                
                results.Add(id, new Secret(id, name, secret, DateTime.Parse(createdAt, null, DateTimeStyles.RoundtripKind)));
            }
            
            return results;
        }
        catch (Exception e)
        {
            Plugin.Log.Error($"[DatabaseInfrastructure.GetSecrets] {e}");
            return null;
        }
    }

    /// <summary>
    ///     Counts all the characters using this secret
    /// </summary>
    public async Task<int> GetCharacterUsingSecretCount(long secretId)
    {
        try
        {
            await using var command = _database.CreateCommand();
            command.CommandText = "SELECT COUNT(*) FROM Characters WHERE SecretId = @SecretId";
            command.Parameters.AddWithValue("@SecretId", secretId);

            return Convert.ToInt32(await command.ExecuteScalarAsync().ConfigureAwait(false));
        }
        catch (Exception e)
        {
            Plugin.Log.Error($"[DatabaseInfrastructure.GetCharacterUsingSecretCount] {e}");
            return 0;
        }
    }

    /// <summary>
    ///     Adds a secret
    /// </summary>
    public async Task<Secret?> AddSecret(string secretName, string secretValue)
    {
        try
        {
            var now = DateTime.UtcNow;
            
            await using var command = _database.CreateCommand();
            command.CommandText = "INSERT INTO Secrets (Name, Secret, CreatedAt) VALUES (@SecretName, @SecretValue, @CreatedAt) RETURNING Id";
            command.Parameters.AddWithValue("@SecretName", secretName);
            command.Parameters.AddWithValue("@SecretValue", secretValue);
            command.Parameters.AddWithValue("@CreatedAt", now.ToString("O"));
            
            var id = Convert.ToInt64(await command.ExecuteScalarAsync().ConfigureAwait(false));
            return new Secret(id, secretName, secretValue, now);
        }
        catch (Exception e)
        {
            Plugin.Log.Error($"[DatabaseInfrastructure.AddSecret] {e}");
            return null;
        }
    }
    
    /// <summary>
    ///     Removes a note for a friend, if it exists
    /// </summary>
    public async Task<bool> RemoveSecret(long secretId)
    {
        try
        {
            await using var command = _database.CreateCommand();
            command.CommandText = "DELETE FROM Secrets WHERE Id = @SecretId";
            command.Parameters.AddWithValue("@SecretId", secretId);

            // Gracefully return true if a secret wasn't deleted
            return await command.ExecuteNonQueryAsync().ConfigureAwait(false) is 0 or 1;
        }
        catch (Exception e)
        {
            Plugin.Log.Error($"[DatabaseInfrastructure.RemoveSecret] {e}");
            return false;
        }
    }

    /// <summary>
    ///     An AR secret
    /// </summary>
    public record Secret(long Id, string Name, string Value, DateTime CreatedAt);
}