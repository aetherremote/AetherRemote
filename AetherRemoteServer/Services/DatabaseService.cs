using AetherRemoteCommon.Domain;
using AetherRemoteCommon.Domain.Enums;
using AetherRemoteCommon.Domain.Enums.Permissions;
using AetherRemoteServer.Domain;
using AetherRemoteServer.Domain.Interfaces;
using AetherRemoteServer.Domain.Shared;
using Microsoft.Data.Sqlite;
// ReSharper disable RedundantBoolCompare

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
        await using var transaction = (SqliteTransaction)await _database.BeginTransactionAsync();

        try
        {
            // Result object awaiting population
            DatabaseResultEc result;
            
            // Initial add command
            var command = _database.CreateCommand();
            command.Transaction = transaction;
            command.CommandText = 
                 """
                    INSERT INTO Permissions_New (FriendCode, TargetFriendCode, PrimaryAllowMask, PrimaryDenyMask, SpeakAllowMask, SpeakDenyMask, ElevatedAllowMask, ElevatedDenyMask)
                    SELECT @friendCode, @targetFriendCode, 0, 0, 0, 0, 0, 0
                    WHERE EXISTS (
                        SELECT 1 FROM Accounts WHERE FriendCode = @targetFriendCode
                    )
                 """;
            command.Parameters.AddWithValue("@friendCode", senderFriendCode);
            command.Parameters.AddWithValue("@targetFriendCode", targetFriendCode);
            
            // If nothing was added, that means we're already friends or friend code doesn't exist
            if (await command.ExecuteNonQueryAsync() is 0)
            {
                // Check to see if the friend code exists, SenderAccountId will always exist because it is a requirement to connect and use the plugin
                var failure = _database.CreateCommand();
                failure.Transaction = transaction;
                failure.CommandText = "SELECT 1 FROM Accounts WHERE FriendCode = @targetFriendCode LIMIT 1";
                failure.Parameters.AddWithValue("@targetFriendCode", targetFriendCode);
                result = await failure.ExecuteScalarAsync() is null ? DatabaseResultEc.NoSuchFriendCode : DatabaseResultEc.AlreadyFriends;
            }
            else
            {
                // Otherwise, check to see if they added us back
                var pair = _database.CreateCommand();
                pair.Transaction = transaction;
                pair.CommandText = "SELECT 1 FROM Permissions WHERE FriendCode = @targetFriendCode AND TargetFriendCode = @senderFriendCode LIMIT 1";
                pair.Parameters.AddWithValue("@senderFriendCode", senderFriendCode);
                pair.Parameters.AddWithValue("@targetFriendCode", targetFriendCode);
                result = await pair.ExecuteScalarAsync() is null ? DatabaseResultEc.Pending : DatabaseResultEc.Success;
            }
            
            // Commit changes and return what happened
            await transaction.CommitAsync();
            return result;
        }
        catch (Exception e)
        {
            _logger.LogError("[AddOrAcceptFriendship] {Error}", e);
            await transaction.RollbackAsync();
            return DatabaseResultEc.Unknown;
        }
    }

    /// <summary>
    ///     Updates a set of permissions between sender and target friend codes
    /// </summary>
    public async Task<DatabaseResultEc> UpdatePermissions(string senderFriendCode, string targetFriendCode, RawPermissions permissions)
    {
        await using var command = _database.CreateCommand();
        command.CommandText = 
            """
                UPDATE Permissions 
                SET 
                    PrimaryAllowMask = @primaryAllow, 
                    PrimaryDenyMask = @primaryDeny, 
                    SpeakAllowMask = @speakAllow, 
                    SpeakDenyMask = @speakDeny, 
                    ElevatedAllowMask = @elevatedAllow, 
                    ElevatedDenyMask = @elevatedDeny
                WHERE FriendCode = @senderFriendCode AND TargetFriendCode = @targetFriendCode
            """;
        command.Parameters.AddWithValue("@senderFriendCode", senderFriendCode);
        command.Parameters.AddWithValue("@targetFriendCode", targetFriendCode);
        command.Parameters.AddWithValue("@primaryAllow", permissions.PrimaryAllow);
        command.Parameters.AddWithValue("@primaryDeny", permissions.PrimaryDeny);
        command.Parameters.AddWithValue("@speakAllow", permissions.SpeakAllow);
        command.Parameters.AddWithValue("@speakDeny", permissions.SpeakDeny);
        command.Parameters.AddWithValue("@elevatedAllow", permissions.ElevatedAllow);
        command.Parameters.AddWithValue("@elevatedDeny", permissions.ElevatedDeny);

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
    ///     <inheritdoc cref="IDatabaseService.GetPermissions"/>
    /// </summary>
    public async Task<UserPermissions?> GetPermissions(string friendCode, string targetFriendCode)
    {   
        await using var command = _database.CreateCommand();
        command.CommandText = 
            """
                SELECT PrimaryPermissions, SpeakPermissions, ElevatedPermissions 
                FROM Permissions 
                WHERE FriendCode = @friendCode AND TargetFriendCode = @targetFriendCode LIMIT 1
            """;
        command.Parameters.AddWithValue("@friendCode", friendCode);
        command.Parameters.AddWithValue("@targetFriendCode", targetFriendCode);

        try
        {
            await using var reader = await command.ExecuteReaderAsync();
            if (await reader.ReadAsync() is false)
                return null;
            
            var primary = (PrimaryPermissions2)reader.GetInt32(0);
            var speak = (SpeakPermissions2)reader.GetInt32(1);
            var elevated = (ElevatedPermissions)reader.GetInt32(2);
            return new UserPermissions(primary, speak, elevated);
        }
        catch (Exception e)
        {
            _logger.LogError("[GetPermissions] {Error}", e);
            return null;
        }
    }

    /// <summary>
    ///     <inheritdoc cref="IDatabaseService.GetAllPermissions"/>
    /// </summary>
    public async Task<List<TwoWayPermissions>> GetAllPermissions(string friendCode)
    {
        await using var command = _database.CreateCommand();
        command.CommandText =
            """
                SELECT
                p.TargetFriendCode,
                p.PrimaryPermissions,
                p.SpeakPermissions,
                p.ElevatedPermissions,
                r.PrimaryPermissions AS PrimaryPermissionsToUs,
                r.SpeakPermissions AS SpeakPermissionsToUs,
                r.ElevatedPermissions AS ElevatedPermissionsToUs
                FROM Permissions AS p LEFT JOIN Permissions AS r ON r.FriendCode = p.TargetFriendCode AND r.TargetFriendCode = p.FriendCode
                WHERE p.FriendCode = @friendCode;
            """;
        command.Parameters.AddWithValue("@friendCode", friendCode);

        var results = new List<TwoWayPermissions>();
        
        try
        {
            await using var reader = await command.ExecuteReaderAsync();
            while (reader.Read())
            {
                // Always get the friend code
                var targetFriendCode = reader.GetString(0);
                
                // Get the permissions we've granted to them
                var primary = (PrimaryPermissions2)reader.GetInt32(1);
                var speak = (SpeakPermissions2)reader.GetInt32(2);
                var elevated = (ElevatedPermissions)reader.GetInt32(3);

                // If 4 is null, 5 and 6 will also be null because that means they do not have permissions for us
                if (reader.IsDBNull(4))
                {
                    results.Add(new TwoWayPermissions(friendCode, targetFriendCode, primary, speak, elevated));
                    continue;
                }
                
                // Get the permissions they've granted to us
                var primary2 = (PrimaryPermissions2)reader.GetInt32(4);
                var speak2 = (SpeakPermissions2)reader.GetInt32(5);
                var elevated2 = (ElevatedPermissions)reader.GetInt32(6);
                results.Add(new TwoWayPermissions(friendCode, targetFriendCode, primary, speak, elevated, primary2, speak2, elevated2));
            }

            return results;
        }
        catch (Exception e)
        {
            _logger.LogError("[GetAllPermissions] {Error}", e);
            return [];
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

    /// <summary>
    ///     <inheritdoc cref="IDatabaseService.UpdateGlobalPermissions"/>
    /// </summary>
    public async Task<DatabaseResultEc> UpdateGlobalPermissions(string senderFriendCode, ResolvedPermissions permissions)
    {
        await using var command = _database.CreateCommand();
        command.CommandText = 
            """
                INSERT INTO GlobalPermissions (FriendCode, PrimaryPermissions, SpeakPermissions, ElevatedPermissions)
                VALUES (@friendCode, @primary, @speak, @elevated)
                ON CONFLICT(FriendCode) DO UPDATE SET PrimaryPermissions = excluded.PrimaryPermissions, SpeakPermissions = excluded.SpeakPermissions, ElevatedPermissions = excluded.ElevatedPermissions;
            """;
        
        command.Parameters.AddWithValue("@friendCode", senderFriendCode);
        command.Parameters.AddWithValue("@primary", permissions.Primary);
        command.Parameters.AddWithValue("@speak", permissions.Speak);
        command.Parameters.AddWithValue("@elevated", permissions.Elevated);

        try
        {
            return await command.ExecuteNonQueryAsync() is 1 ? DatabaseResultEc.Success : DatabaseResultEc.NoOp;
        }
        catch (Exception e)
        {
            _logger.LogWarning("[UpdateGlobalPermissions] {Error}", e);
            return DatabaseResultEc.Unknown;
        }
    }

    /// <summary>
    ///     <inheritdoc cref="IDatabaseService.GetGlobalPermissions"/>
    /// </summary>
    public async Task<ResolvedPermissions?> GetGlobalPermissions(string senderFriendCode)
    {
        await using var command = _database.CreateCommand();
        command.CommandText = 
            """
                INSERT INTO GlobalPermissions (FriendCode, PrimaryPermissions, SpeakPermissions, ElevatedPermissions)
                VALUES (@friendCode, 0, 0, 0)
                ON CONFLICT(FriendCode) DO UPDATE SET FriendCode = FriendCode
                RETURNING PrimaryPermissions, SpeakPermissions, ElevatedPermissions;
            """;
        
        command.Parameters.AddWithValue("@friendCode", senderFriendCode);

        try
        {
            await using var reader = await command.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                var primary = (PrimaryPermissions2)reader.GetInt32(0);
                var speak = (SpeakPermissions2)reader.GetInt32(1);
                var elevated = (ElevatedPermissions)reader.GetInt32(2);
                return new ResolvedPermissions(primary, speak, elevated);
            }
            
            _logger.LogWarning("[GetGlobalPermissions] Command did not return any rows");
            return null;
        }
        catch (Exception e)
        {
            _logger.LogError("[GetGlobalPermissions] {Error}", e);
            return null;
        }
    }

    /// <summary>
    ///     <inheritdoc cref="IDatabaseService.AdminCreateAccount"/>
    /// </summary>
    public async Task<DatabaseResultEc> AdminCreateAccount(ulong discord, string friendCode, string secret)
    {
        await using var command = _database.CreateCommand();
        command.CommandText =
            """
                INSERT OR IGNORE INTO Accounts (DiscordId, FriendCode, Secret, Admin) 
                VALUES (@discord, @friendCode, @secret, 0)
                RETURNING 1;
            """;
        command.Parameters.AddWithValue("@discord", discord);
        command.Parameters.AddWithValue("@friendCode", friendCode);
        command.Parameters.AddWithValue("@secret", secret);

        try
        {
            return await command.ExecuteScalarAsync() is null ? DatabaseResultEc.FriendCodeAlreadyExists : DatabaseResultEc.Success;
        }
        catch (Exception e)
        {
            _logger.LogError("[DatabaseProvider.AdminInsertAccount] Failed to register users for discord id {Discord}, {Error}", discord, e);
            return DatabaseResultEc.Unknown;
        }
    }


    /// <summary>
    ///     <inheritdoc cref="IDatabaseService.AdminGetAccounts"/>
    /// </summary>
    public async Task<List<Account>?> AdminGetAccounts(ulong discord)
    {
        await using var command = _database.CreateCommand();
        command.CommandText = "SELECT * FROM Accounts WHERE DiscordId = @discord";
        command.Parameters.AddWithValue("@discord", discord);
        
        try
        {
            await using var reader = await command.ExecuteReaderAsync();
            
            var results = new List<Account>();
            while (reader.Read())
            {
                var friendCode = reader.GetString(2);
                var secret = reader.GetString(3);
                var admin = reader.GetInt32(4);
                
                results.Add(new Account(friendCode, secret, admin is 1));
            }

            return results;
        }
        catch (Exception e)
        {
            _logger.LogError("[DatabaseProvider.AdminGetAccounts] Failed to get registered users for discord id {Discord}, error: {Error}", discord, e);
            return null;
        }
    }
    
    /// <summary>
    ///     <inheritdoc cref="IDatabaseService.AdminUpdateAccount"/>
    /// </summary>
    public async Task<DatabaseResultEc> AdminUpdateAccount(ulong discord, string oldFriendCode, string newFriendCode)
    {
        await using var command = _database.CreateCommand();
        command.CommandText =
            """
                UPDATE Accounts 
                SET FriendCode = @newFriendCode 
                WHERE DiscordId = @discord AND FriendCode = @friendCode
                RETURNING 1;
            """;
        command.Parameters.AddWithValue("@discord", discord);
        command.Parameters.AddWithValue("@friendCode", oldFriendCode);
        command.Parameters.AddWithValue("@newFriendCode", newFriendCode);

        try
        {
            return await command.ExecuteScalarAsync() is null ? DatabaseResultEc.NoSuchFriendCode : DatabaseResultEc.Success;
        }
        catch (Exception e)
        {
            _logger.LogError("[DatabaseProvider.TryUpdateFriendCode] Failed update user for discord id {Discord}, {Error}", discord, e);
            return DatabaseResultEc.Unknown;
        }
    }
    
    /// <summary>
    ///     <inheritdoc cref="IDatabaseService.AdminDeleteAccount"/>
    /// </summary>
    public async Task<DatabaseResultEc> AdminDeleteAccount(ulong discord, string friendCode)
    {
        await using var command = _database.CreateCommand();
        command.CommandText =
            """
                DELETE FROM Accounts 
                WHERE DiscordId = @discord AND FriendCode = @friendCode
                RETURNING 1
            """;
        command.Parameters.AddWithValue("@discord", discord);
        command.Parameters.AddWithValue("@friendCode", friendCode);
        
        try
        {
            return await command.ExecuteScalarAsync() is null ? DatabaseResultEc.NoSuchFriendCode : DatabaseResultEc.Success;
        }
        catch (Exception e)
        {
            Console.WriteLine($"[DatabaseProvider.TryDeleteFriendCode] Failed update user for discord id {discord}, {e}");
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