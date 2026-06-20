using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AetherRemoteClient.Infrastructure.Database;

public partial class DatabaseInfrastructure
{
    /// <summary>
    ///     Loads all the notes from the table
    /// </summary>
    public async Task<Dictionary<string, string>?> GetNotes()
    {
        try
        {
            await using var command = _database.CreateCommand();
            command.CommandText = "SELECT FriendCode, Note FROM Notes";

            var results = new Dictionary<string, string>();
            
            await using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync().ConfigureAwait(false))
            {
                var friendCode = reader.GetString(0);
                var note = reader.GetString(1);
                
                results.Add(friendCode, note);
            }
            
            return results;
        }
        catch (Exception e)
        {
            Plugin.Log.Error($"[DatabaseInfrastructure.GetNotes] {e}");
            return null;
        }
    }
    
    /// <summary>
    ///     Sets a note for a friend code
    /// </summary>
    /// <param name="friendCode">The friend code of the person the note will be for</param>
    /// <param name="note">The note for the friend code</param>
    public async Task<bool> SetNote(string friendCode, string note)
    {
        try
        {
            await using var command = _database.CreateCommand();
            command.CommandText = "INSERT INTO Notes VALUES (@FriendCode, @Note) ON CONFLICT (FriendCode) DO UPDATE SET Note = @Note";
            command.Parameters.AddWithValue("@FriendCode", friendCode);
            command.Parameters.AddWithValue("@Note", note);

            return await command.ExecuteNonQueryAsync().ConfigureAwait(false) is 1;
        }
        catch (Exception e) 
        {
            Plugin.Log.Error($"[DatabaseInfrastructure.SetNote] {e}");
            return false;
        }
    }
    
    /// <summary>
    ///     Removes a note for a friend, if it exists
    /// </summary>
    public async Task<bool> RemoveNote(string friendCode)
    {
        try
        {
            await using var command = _database.CreateCommand();
            command.CommandText = "DELETE FROM Notes WHERE FriendCode = @FriendCode";
            command.Parameters.AddWithValue("@FriendCode", friendCode);

            // Gracefully return true if a note wasn't deleted
            return await command.ExecuteNonQueryAsync().ConfigureAwait(false) is 0 or 1;
        }
        catch (Exception e)
        {
            Plugin.Log.Error($"[DatabaseInfrastructure.RemoveNote] {e}");
            return false;
        }
    }
}