using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AetherRemoteClient.Domain;
using AetherRemoteClient.Domain.Enums;
using Microsoft.Data.Sqlite;

namespace AetherRemoteClient.Infrastructure.Database;

public partial class DatabaseInfrastructure
{
    /// <summary>
    ///     Imports a values from a legacy configuration, adding them to the database and rolling back if any issues occur
    /// </summary>
    public async Task<bool> ImportLegacyConfigurationValues(
        Dictionary<string, string> notes, 
        Dictionary<string, List<Character>> secrets, 
        bool safeMode, 
        bool showOnDtrBar)
    {
        await using var transaction = (SqliteTransaction)await _database.BeginTransactionAsync().ConfigureAwait(false);

        // Roll back if any fails so that the database isn't in a partial state and the error can be examined
        if (await ImportSettings(transaction, safeMode, showOnDtrBar).ConfigureAwait(false) is false || 
            await ImportNotes(transaction, notes).ConfigureAwait(false) is false || 
            await ImportSecretsWithCharacters(transaction, secrets).ConfigureAwait(false) is false)
        {
            transaction.Rollback();
            return false;
        }

        await transaction.CommitAsync().ConfigureAwait(false);
        return true;
    }
    
    /// <summary>
    ///     Imports notes from a legacy configuration file
    /// </summary>
    private async Task<bool> ImportNotes(SqliteTransaction transaction, Dictionary<string, string> notes)
    {
        try
        {
            await using var command = _database.CreateCommand();
            command.Transaction = transaction;
            command.CommandText = "INSERT INTO Notes VALUES (@FriendCode, @Note) ON CONFLICT (FriendCode) DO UPDATE SET Note = @Note";
            
            var friendCodeParameter = command.Parameters.Add("@FriendCode", SqliteType.Text);
            var noteParameter = command.Parameters.Add("@Note", SqliteType.Text);

            var rows = 0;
            foreach (var (friendCode, note) in notes)
            {
                friendCodeParameter.Value = friendCode;
                noteParameter.Value = note;
                
                rows += await command.ExecuteNonQueryAsync().ConfigureAwait(false);
            }
            
            return rows == notes.Count;
        }
        catch (Exception e) 
        {
            Plugin.Log.Error($"[DatabaseInfrastructure.ImportNotes] {e}");
            return false;
        }
    }
    
    /// <summary>
    ///     Imports notes from a legacy configuration file
    /// </summary>
    private async Task<bool> ImportSettings(SqliteTransaction transaction, bool safeMode, bool showOnDtrBar)
    {
        try
        {
            await using var command = _database.CreateCommand();
            command.Transaction = transaction;
            command.CommandText = "INSERT INTO Settings VALUES (@SettingId, @Value) ON CONFLICT (SettingId) DO UPDATE SET Value = @Value";
            
            var settingIdParameter = command.Parameters.Add("@SettingId", SqliteType.Integer);
            var valueParameter = command.Parameters.Add("@Value", SqliteType.Text);

            var successes = 0;
            
            settingIdParameter.Value = Settings.SafeMode;
            valueParameter.Value = safeMode.ToString();
            successes += await command.ExecuteNonQueryAsync().ConfigureAwait(false);
            
            settingIdParameter.Value = Settings.ShowOnDtrBar;
            valueParameter.Value = showOnDtrBar.ToString();
            successes += await command.ExecuteNonQueryAsync().ConfigureAwait(false);
            
            return successes is 2;
        }
        catch (Exception e) 
        {
            Plugin.Log.Error($"[DatabaseInfrastructure.ImportSettings] {e}");
            return false;
        }
    }

    /// <summary>
    ///     Imports secrets and their associated characters from a legacy configuration file
    /// </summary>
    private async Task<bool> ImportSecretsWithCharacters(SqliteTransaction transaction, Dictionary<string, List<Character>> secrets)
    {
        try
        {
            await using var insertSecretCommand = _database.CreateCommand();
            insertSecretCommand.Transaction = transaction;
            insertSecretCommand.CommandText = "INSERT INTO Secrets (Name, Secret, CreatedAt) VALUES (@Name, @Secret, @CreatedAt) ON CONFLICT (Secret) DO NOTHING RETURNING Id";
            insertSecretCommand.Parameters.AddWithValue("@CreatedAt", DateTimeOffset.UtcNow.ToString("O"));
            var insertSecretName = insertSecretCommand.Parameters.Add("@Name", SqliteType.Text);
            var insertSecretSecret = insertSecretCommand.Parameters.Add("@Secret", SqliteType.Text);

            await using var selectSecretCommand = _database.CreateCommand();
            selectSecretCommand.Transaction = transaction;
            selectSecretCommand.CommandText = "SELECT Id FROM Secrets WHERE Secret = @Secret LIMIT 1";
            var selectSecretSecret = selectSecretCommand.Parameters.Add("@Secret", SqliteType.Text);

            await using var createCharacterConfigurationCommand = _database.CreateCommand();
            createCharacterConfigurationCommand.Transaction = transaction;
            createCharacterConfigurationCommand.CommandText = "INSERT INTO Characters (Name, World, SecretId) VALUES (@Name, @World, @SecretId)";
            var createCharacterConfigurationName = createCharacterConfigurationCommand.Parameters.Add("@Name", SqliteType.Text);
            var createCharacterConfigurationWorld = createCharacterConfigurationCommand.Parameters.Add("@World", SqliteType.Text);
            var createCharacterConfigurationSecretId = createCharacterConfigurationCommand.Parameters.Add("@SecretId", SqliteType.Integer);

            await using var setSecretSettingCommand = _database.CreateCommand();
            setSecretSettingCommand.Transaction = transaction;
            setSecretSettingCommand.CommandText = "INSERT INTO SecretSettings VALUES (@SecretId, @SettingId, @Value) ON CONFLICT (SecretId, SettingId) DO UPDATE SET Value = @Value";
            var setSettingSecretId = setSecretSettingCommand.Parameters.Add("@SecretId", SqliteType.Integer);
            var setSettingSettingId = setSecretSettingCommand.Parameters.Add("@SettingId", SqliteType.Integer);
            var setSettingValue = setSecretSettingCommand.Parameters.Add("@Value", SqliteType.Text);

            var counter = 1;
            foreach (var (secret, characters) in secrets)
            {
                insertSecretName.Value = $"Imported Secret {counter++}";
                insertSecretSecret.Value = secret;
                var result = await insertSecretCommand.ExecuteScalarAsync().ConfigureAwait(false);

                long secretId;
                if (result is null || result == DBNull.Value)
                {
                    selectSecretSecret.Value = secret;
                    secretId = Convert.ToInt64(await selectSecretCommand.ExecuteScalarAsync().ConfigureAwait(false));
                }
                else
                {
                    secretId = Convert.ToInt64(result);
                }

                foreach (var character in characters)
                {
                    createCharacterConfigurationName.Value = character.Name;
                    createCharacterConfigurationWorld.Value = character.World;
                    createCharacterConfigurationSecretId.Value = secretId;
                    await createCharacterConfigurationCommand.ExecuteNonQueryAsync().ConfigureAwait(false);
                }

                setSettingSecretId.Value = secretId;
                
                setSettingSettingId.Value = SecretSetting.AutoLogin;
                setSettingValue.Value = false.ToString();
                await setSecretSettingCommand.ExecuteNonQueryAsync().ConfigureAwait(false);
            }

            return true;
        }
        catch (Exception e)
        {
            Plugin.Log.Error($"[DatabaseInfrastructure.ImportSecretsWithCharacters] {e}");
            return false;
        }
    }
}