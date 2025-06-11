using AetherRemoteCommon.Domain;
using AetherRemoteCommon.V2.Domain.Enum;
using AetherRemoteCommon.Domain.Enums.New;
using AetherRemoteServer.Domain;
using AetherRemoteServer.Domain.Interfaces;
using Microsoft.Data.Sqlite;

namespace AetherRemoteServer.Services;

/// <summary>
///     Provides methods for interacting with the underlying Sqlite3 database
/// </summary>
public class DatabaseService : IDatabaseService
{
    // Constants
    private const string ValidUsersTable = "ValidUsersTable";
    private const string PermissionsTable = "PermissionsTable";
    
    private const string PermissionsTableV2 = "PermissionsTableV2";
    private const string SpeakPermissionsParam = "@SpeakPermissions";
    
    private const string SecretParam = "@Secret";
    private const string FriendCodeParam = "@FriendCode";
    private const string TargetFriendCodeParam = "@TargetFriendCode";
    private const string PrimaryPermissionsParam = "@PrimaryPermissions";

    // Injected
    private readonly ILogger<DatabaseService> _logger;

    // Instantiated
    private readonly SqliteConnection _db;
    private readonly TypedMemoryCache<FriendPermissions> _permissionsCache;

    /// <summary>
    ///     <inheritdoc cref="DatabaseService"/>
    /// </summary>
    public DatabaseService(ILogger<DatabaseService> logger)
    {
        // Inject
        _logger = logger;

        // Db file check
        var path = Path.Combine(Directory.GetCurrentDirectory(), "db", "main2.db");
        if (File.Exists(path) is false)
        {
            logger.LogInformation("Db directory {Directory} does not exist, creating!", path);
            Directory.CreateDirectory(Path.Combine(Directory.GetCurrentDirectory(), "db"));
            File.WriteAllBytes(path, []);
        }

        // Open Db
        _db = new SqliteConnection($"Data Source={path}");
        _db.Open();
        _permissionsCache = new TypedMemoryCache<FriendPermissions>();

        // Table validation
        InitializeDbTables();
    }

    /// <summary>
    ///     Gets a user entry from the valid users table by secret 
    /// </summary>
    public async Task<string?> GetFriendCodeBySecret(string secret)
    {
        await using var command = _db.CreateCommand();
        command.CommandText = $"SELECT * FROM {ValidUsersTable} WHERE Secret = {SecretParam}";
        command.Parameters.AddWithValue(SecretParam, secret);

        try
        {
            await using var reader = await command.ExecuteReaderAsync();
            return await reader.ReadAsync() ? reader.GetString(0) : null;
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
        await using var command = _db.CreateCommand();
        command.CommandText =
            $"""
                 INSERT INTO {PermissionsTableV2} (FriendCode, TargetFriendCode, PrimaryPermissions, SpeakPermissions)
                 VALUES ({FriendCodeParam}, {TargetFriendCodeParam}, {PrimaryPermissionsParam}, {SpeakPermissionsParam})
             """;
        command.Parameters.AddWithValue(FriendCodeParam, senderFriendCode);
        command.Parameters.AddWithValue(TargetFriendCodeParam, targetFriendCode);
        command.Parameters.AddWithValue(PrimaryPermissionsParam, PrimaryPermissions2.None);
        command.Parameters.AddWithValue(SpeakPermissionsParam, SpeakPermissions2.None);

        try
        {
            return await command.ExecuteNonQueryAsync() is 1 ? DatabaseResultEc.Success : DatabaseResultEc.NoOp;
        }
        catch (SqliteException e)
        {
            _logger.LogWarning("Unable to create {FriendCode}'s permissions for {TargetFriendCode}, {Exception}",
                senderFriendCode, targetFriendCode, e.Message);
            
            // Constraint
            if (e.SqliteErrorCode is not 19) 
                return DatabaseResultEc.Unknown;

            return e.SqliteExtendedErrorCode switch
            {
                // Foreign Key
                787 => DatabaseResultEc.NoSuchFriendCode,
                // Unique
                2067 => DatabaseResultEc.AlreadyFriends,
                _ => DatabaseResultEc.Unknown
            };
        }
        catch (Exception e)
        {
            _logger.LogWarning("Unable to create {FriendCode}'s permissions for {TargetFriendCode}, {Exception}",
                senderFriendCode, targetFriendCode, e.Message);
            return DatabaseResultEc.Unknown;
        }
    }

    /// <summary>
    ///     Updates a set of permissions between sender and target friend codes
    /// </summary>
    public async Task<DatabaseResultEc> UpdatePermissions(string senderFriendCode, string targetFriendCode,
        UserPermissions permissions)
    {
        await using var command = _db.CreateCommand();
        command.CommandText =
            $"""
                 UPDATE {PermissionsTableV2} 
                 SET PrimaryPermissions = {PrimaryPermissionsParam}, SpeakPermissions = {SpeakPermissionsParam} 
                 WHERE FriendCode = {FriendCodeParam} AND TargetFriendCode = {TargetFriendCodeParam}
             """;
        command.Parameters.AddWithValue(PrimaryPermissionsParam, permissions.Primary);
        command.Parameters.AddWithValue(SpeakPermissionsParam, permissions.Speak);
        command.Parameters.AddWithValue(FriendCodeParam, senderFriendCode);
        command.Parameters.AddWithValue(TargetFriendCodeParam, targetFriendCode);

        try
        {
            var success = await command.ExecuteNonQueryAsync() is 1;
            if (success)
                _permissionsCache.Remove(senderFriendCode);

            return success ? DatabaseResultEc.Success : DatabaseResultEc.NoOp;
        }
        catch (Exception e)
        {
            _logger.LogWarning("Unable to update {FriendCode}'s permissions for {TargetFriendCode}, {Exception}",
                senderFriendCode, targetFriendCode, e.Message);
            return DatabaseResultEc.Unknown;
        }
    }

    /// <summary>
    ///     Gets all the permissions sender has granted to others
    /// </summary>
    public async Task<FriendPermissions> GetPermissions(string friendCode)
    {
        if (_permissionsCache.Get(friendCode) is { } cachedPermissions)
            return cachedPermissions;

        await using var command = _db.CreateCommand();
        command.CommandText =
            $"""
                SELECT TargetFriendCode, PrimaryPermissions, SpeakPermissions 
                FROM {PermissionsTableV2} 
                WHERE FriendCode={FriendCodeParam}
             """;
        command.Parameters.AddWithValue(FriendCodeParam, friendCode);

        var result = new Dictionary<string, UserPermissions>();
        try
        {
            await using var reader = await command.ExecuteReaderAsync();
            while (reader.Read())
            {
                var targetFriendCode = reader.GetString(0);
                var primary = reader.GetInt32(1);
                var speak = reader.GetInt32(2);
                
                var permissions = new UserPermissions((PrimaryPermissions2)primary, (SpeakPermissions2)speak);
                result.Add(targetFriendCode, permissions);
            }

            var friendPermissions = new FriendPermissions { Permissions = result };
            _permissionsCache.Set(friendCode, friendPermissions);
            return friendPermissions;
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
        await using var command = _db.CreateCommand();
        command.CommandText =
            $"""
                DELETE FROM {PermissionsTableV2} 
                WHERE FriendCode={FriendCodeParam} AND TargetFriendCode={TargetFriendCodeParam}
             """;
        command.Parameters.AddWithValue(FriendCodeParam, senderFriendCode);
        command.Parameters.AddWithValue(TargetFriendCodeParam, targetFriendCode);

        try
        {
            if (await command.ExecuteNonQueryAsync() is not 1)
                return DatabaseResultEc.NoOp;
            
            _permissionsCache.Remove(senderFriendCode);
            return DatabaseResultEc.Success;
        }
        catch (Exception e)
        {
            _logger.LogWarning("Unable to delete {FriendCode}'s permissions for {TargetFriendCode}, {Exception}",
                senderFriendCode, targetFriendCode, e);
            return DatabaseResultEc.Unknown;
        }
    }

    private void InitializeDbTables()
    {
        using var initializerValidUsersTable = _db.CreateCommand();
        initializerValidUsersTable.CommandText =
            $"""
                 CREATE TABLE IF NOT EXISTS {ValidUsersTable} (
                     FriendCode TEXT PRIMARY KEY,
                     Secret TEXT NOT NULL UNIQUE,
                     IsAdmin INTEGER NOT NULL
                 )
             """;

        initializerValidUsersTable.ExecuteNonQuery();

        /*
         * This can be slightly confusing at first.
         *
         * The idea is that this table contains a mapping of
         * user A to user B as the primary key, and then the
         * permissions that user A is granting to user B as
         * an integer that is converted to UserPermissions.
         *
         * The absence of permissions from user A to user B
         * means that user A has not added user B.
         */
        using var initializePermissionsTable = _db.CreateCommand();
        initializePermissionsTable.CommandText =
            $"""
                 CREATE TABLE IF NOT EXISTS {PermissionsTable} (
                     UserFriendCode TEXT NOT NULL,
                     TargetFriendCode TEXT NOT NULL,
                     Version INTEGER NOT NULL,
                     PrimaryPermissions INTEGER NOT NULL,
                     LinkshellPermissions INTEGER NOT NULL,
                     PRIMARY KEY (UserFriendCode, TargetFriendCode),
                     FOREIGN KEY (UserFriendCode) REFERENCES {ValidUsersTable}(FriendCode),
                     FOREIGN KEY (TargetFriendCode) REFERENCES {ValidUsersTable}(FriendCode)
                 )
             """;

        initializePermissionsTable.ExecuteNonQuery();
        
        using var initializePermissionsTableV2 = _db.CreateCommand();
        initializePermissionsTableV2.CommandText = 
            $"""
                CREATE TABLE IF NOT EXISTS {PermissionsTableV2} (
                     FriendCode TEXT NOT NULL,
                     TargetFriendCode TEXT NOT NULL,
                     PrimaryPermissions INTEGER NOT NULL,
                     SpeakPermissions INTEGER NOT NULL,
                     PRIMARY KEY (FriendCode, TargetFriendCode),
                     FOREIGN KEY (FriendCode) REFERENCES {ValidUsersTable} (FriendCode),
                     FOREIGN KEY (TargetFriendCode) REFERENCES {ValidUsersTable} (FriendCode)
                )
             """;
        
        initializePermissionsTableV2.ExecuteNonQuery();
    }
}