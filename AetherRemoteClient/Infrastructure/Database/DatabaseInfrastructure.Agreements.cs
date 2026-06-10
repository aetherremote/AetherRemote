using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AetherRemoteClient.Infrastructure.Database;

public partial class DatabaseInfrastructure
{
    /// <summary>
    ///     Loads all the agreements from the table
    /// </summary>
    public async Task<Dictionary<string, bool>?> GetAgreements()
    {
        try
        {
            await using var command = _database.CreateCommand();
            command.CommandText = "SELECT Name, Agreed FROM Agreements";

            var results = new Dictionary<string, bool>();
            
            await using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync().ConfigureAwait(false))
            {
                var name = reader.GetString(0);
                var agreed = reader.GetBoolean(1);
                
                results.Add(name, agreed);
            }
            
            return results;
        }
        catch (Exception e)
        {
            Plugin.Log.Error($"[DatabaseInfrastructure.GetAgreements] {e}");
            return null;
        }
    }

    /// <summary>
    ///     Set the value of an agreement to be agreed or not
    /// </summary>
    /// <param name="name">The name of the agreement</param>
    /// <param name="value">Whether the agreement has been agreed to or not</param>
    public async Task<bool> SetAgreement(string name, bool value)
    {
        try
        {
            await using var command = _database.CreateCommand();
            command.CommandText = "INSERT INTO Agreements VALUES (@Name, @Value)";
            command.Parameters.AddWithValue("@Name", name);
            command.Parameters.AddWithValue("@Value", value);

            return await command.ExecuteNonQueryAsync().ConfigureAwait(false) is 1;
        }
        catch (Exception e)
        {
            Plugin.Log.Error($"[DatabaseInfrastructure.SetAgreement] {e}");
            return false;
        }
    }
}