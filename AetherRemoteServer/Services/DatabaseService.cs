using AetherRemoteCommon.Domain;
using AetherRemoteCommon.Domain.Enums;
using AetherRemoteCommon.Domain.Enums.Permissions;
using AetherRemoteServer.Domain;
using AetherRemoteServer.Domain.Interfaces;
using Microsoft.Data.Sqlite;

namespace AetherRemoteServer.Services;

/// <summary>
///     Provides methods for interacting with the underlying Sqlite3 database
/// </summary>
public class DatabaseService : IDatabaseService
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

    /// <summary>
    ///     Creates an empty set of permissions between sender and target friend codes
    /// </summary>
    public async Task<DatabaseResultEc> CreatePermissions(string senderFriendCode, string targetFriendCode)
    {
        await using var command = _database.CreateCommand();
        command.CommandText = "INSERT INTO Permissions VALUES (@friendCode, @targetFriendCode, @primary, @speak, @elevated)";
        command.Parameters.AddWithValue("@friendCode", senderFriendCode);
        command.Parameters.AddWithValue("@targetFriendCode", targetFriendCode);
        command.Parameters.AddWithValue("@primary", PrimaryPermissions2.None);
        command.Parameters.AddWithValue("@speak", SpeakPermissions2.None);
        command.Parameters.AddWithValue("@elevated", ElevatedPermissions.None);

        try
        {
            return await command.ExecuteNonQueryAsync() is 0 ? DatabaseResultEc.NoOp : DatabaseResultEc.Success;
        }
        catch (SqliteException e)
        {
            _logger.LogWarning("Unable to create {FriendCode}'s permissions for {TargetFriendCode}, {Exception}", senderFriendCode, targetFriendCode, e.Message);
            
            // Constraint
            if (e.SqliteErrorCode is not 19)
            {
                _logger.LogWarning("{User} failed to add {Target} due to unknown error {Exception} - {ExtendedException}", senderFriendCode, targetFriendCode, e.SqliteErrorCode, e.SqliteExtendedErrorCode);
                return DatabaseResultEc.Unknown;
            }

            switch (e.SqliteExtendedErrorCode)
            {
                case 787:
                    return DatabaseResultEc.NoSuchFriendCode;
                
                case 2067:
                    return DatabaseResultEc.AlreadyFriends;
                
                default:
                    _logger.LogWarning("{User} failed to add {Target} due to unknown error sqlite 19 error {Exception} - {ExtendedException}", senderFriendCode, targetFriendCode, e.SqliteErrorCode, e.SqliteExtendedErrorCode);
                    return DatabaseResultEc.Unknown;
            }
        }
        catch (Exception e)
        {
            _logger.LogWarning("Unable to create {FriendCode}'s permissions for {TargetFriendCode}, {Exception}", senderFriendCode, targetFriendCode, e.Message);
            return DatabaseResultEc.Unknown;
        }
    }

    /// <summary>
    ///     Updates a set of permissions between sender and target friend codes
    /// </summary>
    public async Task<DatabaseResultEc> UpdatePermissions(string senderFriendCode, string targetFriendCode, UserPermissions permissions)
    {
        await using var command = _database.CreateCommand();
        command.CommandText = "UPDATE Permissions SET PrimaryPermissions = @primary, SpeakPermissions = @speak, ElevatedPermissions = @elevated WHERE FriendCode = @friendCode AND TargetFriendCode = @targetFriendCode";
        command.Parameters.AddWithValue("@primary", permissions.Primary);
        command.Parameters.AddWithValue("@speak", permissions.Speak);
        command.Parameters.AddWithValue("@elevated", permissions.Elevated);
        command.Parameters.AddWithValue("@friendCode", senderFriendCode);
        command.Parameters.AddWithValue("@targetFriendCode", targetFriendCode);

        try
        {
            return await command.ExecuteNonQueryAsync() is 1 ? DatabaseResultEc.Success : DatabaseResultEc.NoOp;
        }
        catch (Exception e)
        {
            _logger.LogWarning("Unable to update {FriendCode}'s permissions for {TargetFriendCode}, {Exception}", senderFriendCode, targetFriendCode, e.Message);
            return DatabaseResultEc.Unknown;
        }
    }

    /// <summary>
    ///     Gets all the permissions sender has granted to others
    /// </summary>
    public async Task<FriendPermissions> GetPermissions(string friendCode)
    {
        await using var command = _database.CreateCommand();
        command.CommandText = "SELECT TargetFriendCode, PrimaryPermissions, SpeakPermissions, ElevatedPermissions FROM Permissions WHERE FriendCode = @friendCode";
        command.Parameters.AddWithValue("@friendCode", friendCode);

        var result = new Dictionary<string, UserPermissions>();
        try
        {
            await using var reader = await command.ExecuteReaderAsync();
            while (reader.Read())
            {
                var targetFriendCode = reader.GetString(0);
                var primary = reader.GetInt32(1);
                var speak = reader.GetInt32(2);
                var elevated = reader.GetInt32(3);
                
                var permissions = new UserPermissions((PrimaryPermissions2)primary, (SpeakPermissions2)speak, (ElevatedPermissions)elevated);
                result.Add(targetFriendCode, permissions);
            }
            
            return new FriendPermissions { Permissions = result };
        }
        catch (Exception e)
        {
            _logger.LogWarning("Unable to get permissions for {FriendCode}, {Exception}", friendCode, e.Message);
            return new FriendPermissions();
        }
    }

    /// <summary>
    ///     Deletes a set of permissions between sender and target friend code
    /// </summary>
    public async Task<DatabaseResultEc> DeletePermissions(string senderFriendCode, string targetFriendCode)
    {
        await using var command = _database.CreateCommand();
        command.CommandText = "DELETE FROM Permissions WHERE FriendCode = @friendCode AND TargetFriendCode = @targetFriendCode";
        command.Parameters.AddWithValue("@friendCode", senderFriendCode);
        command.Parameters.AddWithValue("@targetFriendCode", targetFriendCode);

        try
        {
            return await command.ExecuteNonQueryAsync() is 1 ? DatabaseResultEc.Success : DatabaseResultEc.NoOp;
        }
        catch (Exception e)
        {
            _logger.LogWarning("Unable to delete {FriendCode}'s permissions for {TargetFriendCode}, {Exception}", senderFriendCode, targetFriendCode, e);
            return DatabaseResultEc.Unknown;
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