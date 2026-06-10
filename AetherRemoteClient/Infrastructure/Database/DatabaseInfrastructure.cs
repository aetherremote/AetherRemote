using System;
using System.IO;
using Microsoft.Data.Sqlite;

namespace AetherRemoteClient.Infrastructure.Database;

/// <summary>
///     Exposes methods to access the underlying configuration and data values
/// </summary>
public partial class DatabaseInfrastructure
{
    // Const
    private const string ConfigurationFileName = "Data.db";
    private static readonly string ConfigurationFilePath = Path.Combine(Plugin.PluginInterface.GetPluginConfigDirectory(), ConfigurationFileName);
    
    // Instantiated
    private readonly SqliteConnection _database;
    
    /// <summary>
    ///     <inheritdoc cref="DatabaseInfrastructure"/>
    /// </summary>
    public DatabaseInfrastructure()
    {
        var connection = new SqliteConnectionStringBuilder
        {
            DataSource = ConfigurationFilePath,
            Mode = SqliteOpenMode.ReadWriteCreate,
            ForeignKeys = true
        }.ToString();
        
        _database = new SqliteConnection(connection);
        _database.Open();
        
        InitializeData();
    }
    
    private void InitializeData()
    {
        try
        {
            using var transaction = _database.BeginTransaction();
            foreach (var schema in Schema.Tables)
            {
                using var command = _database.CreateCommand();
                command.Transaction = transaction;
                command.CommandText = schema;
                command.ExecuteNonQuery();
            }
        
            transaction.Commit();
        }
        catch (Exception e)
        {
            Plugin.Log.Error($"[DatabaseInfrastructure.InitializeData] {e}");
        }
    }
    
    /// <summary>
    ///     Provides access to the schema used for the database. Keeping this private since it is the only place such a schema should exist.
    /// </summary>
    private static class Schema
    {
        /// <summary>
        ///     Table schema in required creation order  
        /// </summary>
        public static readonly string[] Tables =
        [
            CreateSecretsTable,
            CreateCharactersTable,
            CreateSettingsTable,
            CreateTransformationsTable,
            CreateAgreementsTable,
            CreateNotesTable
        ];

        private const string CreateAgreementsTable =
            """
                CREATE TABLE IF NOT EXISTS "Agreements" (
            		"Name"	                        TEXT PRIMARY KEY,
            		"Agreed"	                    INTEGER NOT NULL CHECK("Agreed" IN (1, 0))
                );
            """;

        private const string CreateCharactersTable =
            """
                CREATE TABLE IF NOT EXISTS "Characters" (
            	    "Id"	                        INTEGER PRIMARY KEY AUTOINCREMENT,
            	    "Name"	                        TEXT NOT NULL,
            	    "World"	                        TEXT NOT NULL,
            	    "SecretId"	                    INTEGER,
            	    FOREIGN KEY("SecretId") REFERENCES "Secrets"("Id") ON DELETE SET NULL
                )
            """;

        private const string CreateNotesTable =
            """
                CREATE TABLE IF NOT EXISTS "Notes" (
            	    "FriendCode"	                TEXT PRIMARY KEY,
            	    "Note"	                        TEXT NOT NULL
                );
            """;

        private const string CreateSecretsTable =
            """
                CREATE TABLE IF NOT EXISTS "Secrets" (
            	    "Id"	                        INTEGER PRIMARY KEY AUTOINCREMENT,
            	    "Name"	                        TEXT NOT NULL UNIQUE,
            	    "Secret"	                    TEXT NOT NULL UNIQUE
                );
            """;

        private const string CreateSettingsTable =
            """
                CREATE TABLE IF NOT EXISTS "Settings" (
            	    "SecretId"	                    INTEGER NOT NULL,
            	    "Name"	                        TEXT NOT NULL,
            	    "Value"	                        TEXT NOT NULL,
            	    FOREIGN KEY("SecretId") REFERENCES "Secrets"("Id") ON DELETE CASCADE
                );
            """;

        private const string CreateTransformationsTable =
            """
                CREATE TABLE IF NOT EXISTS "Transformations" (
            	    "CharacterId"	                INTEGER NOT NULL,
            	    "Attribute"             	    TEXT NOT NULL,
            	    "Data"	                        BLOB NOT NULL,
            	    FOREIGN KEY("CharacterId") REFERENCES "Characters"("Id") ON DELETE CASCADE
                );
            """;
    }
}