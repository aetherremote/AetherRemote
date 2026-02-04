using AetherRemoteServer.Domain;
using Microsoft.Data.Sqlite;

namespace AetherRemoteServer.Services.Database;

/// <summary>
///     Provides methods for interacting with the underlying Sqlite3 database
/// </summary>
public partial class DatabaseService
{
    // Injected
    private readonly ILogger<DatabaseService> _logger;

    // Instantiated
    private readonly SqliteConnection _database;

    /// <summary>
    ///     <inheritdoc cref="DatabaseService"/>
    /// </summary>
    public DatabaseService(Configuration configuration, ILogger<DatabaseService> logger)
    {
        // Inject
        _logger = logger;

#if DEBUG
        var path = configuration.BetaDatabasePath;
#else
        var path = configuration.ReleaseDatabasePath;
#endif

        var connection = new SqliteConnectionStringBuilder
        {
            Cache = SqliteCacheMode.Shared,
            DataSource = path,
            Mode = SqliteOpenMode.ReadWriteCreate
        }.ToString();

        // Open Db
        _database = new SqliteConnection(connection);
        _database.Open();
        
        // Configure Db
        ConfigureDatabaseConnection();
    }

    /// <summary>
    ///     Gets a user entry from the valid users table by secret 
    /// </summary>
    public async Task<string?> GetFriendCodeBySecret(string secret)
    {
        await using var command = _database.CreateCommand();
        command.CommandText = "SELECT * FROM Accounts WHERE Secret = @secret";
        command.Parameters.AddWithValue("@secret", secret);

        try
        {
            await using var reader = await command.ExecuteReaderAsync();
            return await reader.ReadAsync() ? reader.GetString(2) : null;
        }
        catch (Exception e)
        {
            _logger.LogWarning("Unable to get user with secret {Secret}, {Exception}", secret, e.Message);
            return null;
        }
    }

    private void ConfigureDatabaseConnection()
    {
        try
        {
            using var command = _database.CreateCommand();

            command.CommandText = "PRAGMA journal_mode=WAL;";
            command.ExecuteNonQuery();
        
            command.CommandText = "PRAGMA foreign_keys=ON;";
            command.ExecuteNonQuery();
        
            command.CommandText = "PRAGMA busy_timeout=5000;";
            command.ExecuteNonQuery();
        }
        catch (Exception e)
        {
            _logger.LogError("Unexpected exception while configuring database, {Error}", e);
        }
    }
}