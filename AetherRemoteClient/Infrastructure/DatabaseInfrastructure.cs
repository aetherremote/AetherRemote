using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using AetherRemoteClient.Domain;
using AetherRemoteClient.Domain.Glamourer;
using AetherRemoteClient.Services;
using AetherRemoteClient.Utils;
using AetherRemoteCommon.Domain.Enums;
using Microsoft.Data.Sqlite;
using Newtonsoft.Json.Linq;

namespace AetherRemoteClient.Infrastructure;

/// <summary>
///     Provides access to the local database for plugin settings and configuration storage
/// </summary>
public class DatabaseInfrastructure : IDisposable
{
    // Const
    private const string ConfigurationFileName = "Configuration.db";
    
    private const string CharactersTable = "Characters";
    private const string CharacterConfigurationsTable = "CharacterConfigurations";
    private const string PermanentTransformationsTable = "PermanentTransformations";
    
    private const string NameParam = "@Name";
    private const string WorldParam = "@World";
    private const string IdParam = "@CharacterId";

    private const int Version = 1;
    
    private static readonly string ConfigurationFilePath = Path.Combine(Plugin.PluginInterface.GetPluginConfigDirectory(), ConfigurationFileName);

    // Keep track of the local player's configuration id that is set anytime the GetIdForPlayer method is called
    private int _localCharacterIndex = -1;

    // Instantiated
    private readonly SqliteConnection _connection;

    /// <summary>
    ///     <inheritdoc cref="ConfigurationService"/>
    /// </summary>
    public DatabaseInfrastructure()
    {
        if (File.Exists(ConfigurationFilePath) is false)
        {
            try
            {
                Directory.CreateDirectory(Plugin.PluginInterface.GetPluginConfigDirectory());
                File.WriteAllBytes(ConfigurationFilePath, []);
                Plugin.Log.Error("[DatabaseInfrastructure] Created plugin config directory and settings file");
            }
            catch (Exception e)
            {
                Plugin.Log.Error($"[DatabaseInfrastructure] Unexpected error while creating plugin settings file, {e}");
            }
        }

        _connection = new SqliteConnection($"Data Source={ConfigurationFilePath}");
        Plugin.Log.Info($"Trying path at {ConfigurationFilePath}");
        try
        {
            _connection.Open();
        }
        catch (Exception e)
        {
            Plugin.Log.Error(e.ToString());
        }
        

        InitializeTables();
    }

    public async Task<CharacterConfiguration> GetConfigurationForPlayer(string characterName, string characterWorld)
    {
        var id = await GetIdForPlayer(characterName, characterWorld);
        return id < 0 ? new CharacterConfiguration() : await GetCharacterConfigurationForPlayer(id);
    }

    // TODO
    public async Task<bool> SaveCharacterConfigurationForPlayer(CharacterConfiguration characterConfiguration)
    {
        await Task.Delay(1000);
        return true;
    }

    public async Task<PermanentTransformationData?> GetPermanentTransformationForPlayer(string characterName, string characterWorld)
    {
        var id = await GetIdForPlayer(characterName, characterWorld);
        return id < 0 ? null : await GetPermanentTransformationForPlayer(id);
    }

    // TODO
    public async Task<bool> SavePermanentTransformationForPlayer(PermanentTransformationData permanentTransformation)
    {
        await Task.Delay(1000);
        return true;
    }

    /// <summary>
    ///     Queries the database for the local player's configuration file
    /// </summary>
    /// <returns>The character id for the local player, otherwise -1</returns>
    private async Task<int> GetIdForPlayer(string characterName, string characterWorld)
    {
        await using var command = _connection.CreateCommand();
        command.CommandText = $"SELECT Id FROM {CharactersTable} WHERE Name = {NameParam} AND World = {WorldParam}";
        command.Parameters.AddWithValue(NameParam, characterName);
        command.Parameters.AddWithValue(WorldParam, characterWorld);

        try
        {
            await using var reader = await command.ExecuteReaderAsync();
            _localCharacterIndex = await reader.ReadAsync() ? reader.GetInt32(0) : -1;
            return _localCharacterIndex;
        }
        catch (Exception e)
        {
            Plugin.Log.Warning($"[DatabaseInfrastructure] Unexpected error while getting character id, {e}");
            return -1;
        }
    }

    private async Task<CharacterConfiguration> GetCharacterConfigurationForPlayer(int id)
    {
        await using var command = _connection.CreateCommand();
        command.CommandText = $"SELECT * FROM {CharacterConfigurationsTable} WHERE CharacterId = {IdParam}";
        command.Parameters.AddWithValue(IdParam, id);
        
        try
        {
            await using var reader = await command.ExecuteReaderAsync();
            if (await reader.ReadAsync() is false)
                return new CharacterConfiguration();
            
            var configurationId = reader.GetInt32(0);
            var characterId = reader.GetInt32(1);
            var version = reader.GetInt32(2);
            var autoLogin = reader.GetInt32(3) is 1;
            var secret = reader.GetString(4);

            return new CharacterConfiguration
            {
                ConfigurationId = configurationId,
                CharacterId = characterId,
                Version = version,
                AutoLogin = autoLogin,
                Secret = secret
            };
        }
        catch (Exception e)
        {
            Plugin.Log.Warning($"[DatabaseInfrastructure] Unexpected error while getting character configuration, {e}");
            return new CharacterConfiguration();
        }
    }

    private async Task<PermanentTransformationData?> GetPermanentTransformationForPlayer(int id)
    {
        await using var command = _connection.CreateCommand();
        command.CommandText = $"SELECT * FROM {PermanentTransformationsTable} WHERE CharacterId = {IdParam}";
        command.Parameters.AddWithValue(IdParam, id);
        
        try
        {
            await using var reader = await command.ExecuteReaderAsync();
            if (await reader.ReadAsync() is false)
                return null;
            
            var characterId = reader.GetInt32(0);
            var version = reader.GetInt32(1);
            var sender = reader.GetString(2);
            var glamourerDataRaw = reader.GetString(3);
            var glamourerApplyTypeRaw = reader.GetInt64(4);
            var key = reader.GetString(5);
            var modPathDataRaw = reader.IsDBNull(6) ? null : reader.GetString(6);
            var modMetaData = reader.IsDBNull(7) ? null : reader.GetString(7);
            var customizePlusData = reader.IsDBNull(8) ? null : reader.GetString(8);
            var moodlesData = reader.IsDBNull(9) ? null : reader.GetString(9);

            var glamourerData = JObject.Parse(glamourerDataRaw);
            if (GlamourerDesignHelper.FromJObject(glamourerData) is not { } glamourerDesign)
            {
                Plugin.Log.Warning("[DatabaseInfrastructure] [GetPermanentTransformationForPlayer] GlamourerDesign returned null");
                glamourerDesign = new GlamourerDesign();
            }
            
            var glamourerApplyType = (GlamourerApplyFlags)glamourerApplyTypeRaw;
            var modPathData = modPathDataRaw is null ? null : JObject.Parse(modPathDataRaw).ToObject<Dictionary<string, string>>();

            return new PermanentTransformationData
            {
                // CharacterId = characterId,
                // Version = version,
                Sender = sender,
                GlamourerDesign = glamourerDesign,
                GlamourerApplyType = glamourerApplyType,
                Key = key,
                ModPathData = modPathData,
                ModMetaData = modMetaData,
                CustomizePlusData = customizePlusData,
                MoodlesData = moodlesData
            };
        }
        catch (Exception e)
        {
            Plugin.Log.Warning($"[DatabaseInfrastructure] Unexpected error while getting permanent transformation data, {e}");
            return null;
        }
    }

    private void InitializeTables()
    {
        try
        {
            using var initializeCharacterTable = _connection.CreateCommand();
            initializeCharacterTable.CommandText =
                $"""
                    CREATE TABLE IF NOT EXISTS {CharactersTable} (
                         Id INTEGER PRIMARY KEY AUTOINCREMENT,
                         Name TEXT NOT NULL,
                         World TEXT NOT NULL,
                         UNIQUE(Name, World)
                    );
                 """;

            initializeCharacterTable.ExecuteNonQuery();

            using var initializeCharacterConfigurationTable = _connection.CreateCommand();
            initializeCharacterConfigurationTable.CommandText =
                $"""
                    CREATE TABLE IF NOT EXISTS {CharacterConfigurationsTable} (
                        ConfigurationId INTEGER PRIMARY KEY AUTOINCREMENT,
                        CharacterId INTEGER NOT NULL,
                        Version INTEGER NOT NULL,
                        AutoLogin INTEGER NOT NULL,
                        Secret TEXT NOT NULL,
                        FOREIGN KEY (CharacterId) REFERENCES {CharactersTable}(Id) ON DELETE CASCADE
                    );
                 """;

            initializeCharacterConfigurationTable.ExecuteNonQuery();

            using var initializePermanentTransformationsTable = _connection.CreateCommand();
            initializePermanentTransformationsTable.CommandText =
                $"""
                    CREATE TABLE IF NOT EXISTS {PermanentTransformationsTable} (
                        CharacterId INTEGER PRIMARY KEY,
                        Version INTEGER NOT NULL,
                        Sender TEXT NOT NULL,
                        GlamourerData TEXT NOT NULL,
                        GlamourerApplyType INTEGER NOT NULL,
                        UnlockKey TEXT NOT NULL,
                        ModPathData TEXT,
                        ModMetaData TEXT,
                        CustomizePlusData TEXT,
                        MoodlesData TEXT,
                        FOREIGN KEY (CharacterId) REFERENCES {CharactersTable}(Id) ON DELETE CASCADE
                    );
                 """;

            initializePermanentTransformationsTable.ExecuteNonQuery();
        }
        catch (Exception e)
        {
            Plugin.Log.Error($"[DatabaseInfrastructure] Unexpected error initializing tables, {e}");
        }
    }

    public void Dispose()
    {
        _connection.Close();
        _connection.Dispose();
        GC.SuppressFinalize(this);
    }
}