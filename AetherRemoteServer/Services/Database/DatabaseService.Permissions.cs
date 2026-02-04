using AetherRemoteCommon.Domain;
using AetherRemoteCommon.Domain.Enums;
using AetherRemoteCommon.Domain.Enums.Permissions;
using AetherRemoteCommon.Util;
using AetherRemoteServer.Domain;
using Microsoft.Data.Sqlite;

namespace AetherRemoteServer.Services.Database;

public partial class DatabaseService
{
    /// <summary>
    ///     Gets a raw permission set
    /// </summary>
    /// <param name="friendCode">The owner of the permissions</param>
    /// <param name="targetFriendCode">Who the permissions apply to</param>
    public async Task<RawPermissions?> GetSinglePermissions(string friendCode, string targetFriendCode)
    {   
        await using var command = _database.CreateCommand();
        command.CommandText = 
            """
                SELECT PrimaryAllowMask, PrimaryDenyMask, SpeakAllowMask, SpeakDenyMask, ElevatedAllowMask, ElevatedDenyMask 
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
            
            var otherPrimaryAllow = (PrimaryPermissions2)reader.GetInt32(0);
            var otherPrimaryDeny = (PrimaryPermissions2)reader.GetInt32(1);
            var otherSpeakAllow = (SpeakPermissions2)reader.GetInt32(2);
            var otherSpeakDeny = (SpeakPermissions2)reader.GetInt32(3);
            var otherElevatedAllow = (ElevatedPermissions)reader.GetInt32(4);
            var otherElevatedDeny = (ElevatedPermissions)reader.GetInt32(5);
            return new RawPermissions(otherPrimaryAllow, otherPrimaryDeny, otherSpeakAllow, otherSpeakDeny, otherElevatedAllow, otherElevatedDeny);
        }
        catch (Exception e)
        {
            _logger.LogError("[GetPermissions] {Error}", e);
            return null;
        }
    }
    
    /// <summary>
    ///     Gets all the raw permission sets
    /// </summary>
    /// <param name="friendCode">The owner of all the permissions</param>
    public async Task<List<RawPermissionsGrantedResolvedPermissionGiven>> GetAllPermissions(string friendCode)
    {
        await using var command = _database.CreateCommand();
        command.CommandText =
            """
                SELECT
                p.TargetFriendCode,
                
                -- Permissions we granted to them
                p.PrimaryAllowMask              AS Self_PrimaryAllowMask,
                p.PrimaryDenyMask               AS Self_PrimaryDenyMask,
                p.SpeakAllowMask                AS Self_SpeakAllowMask,
                p.SpeakDenyMask                 AS Self_SpeakDenyMask,
                p.ElevatedAllowMask             AS Self_ElevatedAllowMask,
                p.ElevatedDenyMask              AS Self_ElevatedDenyMask,
                
                -- Permissions they've granted to us
                r.PrimaryAllowMask              AS Other_PrimaryAllowMask,
                r.PrimaryDenyMask               AS Other_PrimaryDenyMask,
                r.SpeakAllowMask                AS Other_SpeakAllowMask,
                r.SpeakDenyMask                 AS Other_SpeakDenyMask,
                r.ElevatedAllowMask             AS Other_ElevatedAllowMask,
                r.ElevatedDenyMask              AS Other_ElevatedDenyMask,
                
                -- Their global permissions
                g_other.PrimaryPermissions      AS Other_GlobalPrimaryPermissions,
                g_other.SpeakPermissions        AS Other_GlobalSpeakPermissions,
                g_other.ElevatedPermissions     AS Other_GlobalElevatedPermissions
                
                FROM Permissions AS p 
                
                LEFT JOIN Permissions AS r 
                    ON r.FriendCode = p.TargetFriendCode 
                    AND r.TargetFriendCode = p.FriendCode
                    
                LEFT JOIN GlobalPermissions AS g_other
                    ON g_other.FriendCode = r.FriendCode                
                
                WHERE p.FriendCode = @friendCode;
            """;
        command.Parameters.AddWithValue("@friendCode", friendCode);
        
        var results = new List<RawPermissionsGrantedResolvedPermissionGiven>();
        
        try
        {
            await using var reader = await command.ExecuteReaderAsync();
            while (reader.Read())
            {
                // Always get the friend code
                var targetFriendCode = reader.GetString(0);
                
                // Get the raw permissions we have granted
                var selfPrimaryAllow = (PrimaryPermissions2)reader.GetInt32(1);
                var selfPrimaryDeny = (PrimaryPermissions2)reader.GetInt32(2);
                var selfSpeakAllow = (SpeakPermissions2)reader.GetInt32(3);
                var selfSpeakDeny = (SpeakPermissions2)reader.GetInt32(4);
                var selfElevatedAllow = (ElevatedPermissions)reader.GetInt32(5);
                var selfElevatedDeny = (ElevatedPermissions)reader.GetInt32(6);
                var selfRawPermissions = new RawPermissions(selfPrimaryAllow, selfPrimaryDeny, selfSpeakAllow, selfSpeakDeny, selfElevatedAllow, selfElevatedDeny);
                
                // If 7 is null, then everything past will also be null
                if (reader.IsDBNull(7))
                {
                    results.Add(new RawPermissionsGrantedResolvedPermissionGiven(targetFriendCode, selfRawPermissions, null));
                    continue;
                }
                
                // Their permissions they have granted to us
                var otherPrimaryAllow = (PrimaryPermissions2)reader.GetInt32(7);
                var otherPrimaryDeny = (PrimaryPermissions2)reader.GetInt32(8);
                var otherSpeakAllow = (SpeakPermissions2)reader.GetInt32(9);
                var otherSpeakDeny = (SpeakPermissions2)reader.GetInt32(10);
                var otherElevatedAllow = (ElevatedPermissions)reader.GetInt32(11);
                var otherElevatedDeny = (ElevatedPermissions)reader.GetInt32(12);
                var otherRawPermissions = new RawPermissions(otherPrimaryAllow, otherPrimaryDeny, otherSpeakAllow, otherSpeakDeny, otherElevatedAllow, otherElevatedDeny);
                
                // Their global permissions to be resolved
                ResolvedPermissions otherResolvedPermissions;
                
                // Check if their global permissions are set (global permissions may not be set due to lazy instantiation)
                if (reader.IsDBNull(13))
                {
                    // Just assign it as empties
                    var otherGlobalPermissions = new ResolvedPermissions(PrimaryPermissions2.None, SpeakPermissions2.None, ElevatedPermissions.None);
                    otherResolvedPermissions = PermissionResolver.Resolve(otherGlobalPermissions, otherRawPermissions);
                }
                else
                {
                    // Read from the rest of the rows
                    var otherGlobalPrimary = (PrimaryPermissions2)reader.GetInt32(13);
                    var otherGlobalSpeak = (SpeakPermissions2)reader.GetInt32(14);
                    var otherGlobalElevated = (ElevatedPermissions)reader.GetInt32(15);
                    var otherGlobalPermissions = new ResolvedPermissions(otherGlobalPrimary, otherGlobalSpeak, otherGlobalElevated);
                
                    // Resolve
                    otherResolvedPermissions = PermissionResolver.Resolve(otherGlobalPermissions, otherRawPermissions);
                }
                
                // Add everything
                results.Add(new RawPermissionsGrantedResolvedPermissionGiven(targetFriendCode, selfRawPermissions, otherResolvedPermissions));
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
                    INSERT INTO Permissions (FriendCode, TargetFriendCode, PrimaryAllowMask, PrimaryDenyMask, SpeakAllowMask, SpeakDenyMask, ElevatedAllowMask, ElevatedDenyMask)
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
}