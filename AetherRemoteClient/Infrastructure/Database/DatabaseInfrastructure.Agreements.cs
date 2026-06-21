using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AetherRemoteClient.Domain.Enums;

namespace AetherRemoteClient.Infrastructure.Database;

public partial class DatabaseInfrastructure
{
    /// <summary>
    ///     Loads all the agreements from the table
    /// </summary>
    public async Task<Dictionary<Agreement, bool>?> GetAgreements()
    {
        try
        {
            await using var command = _database.CreateCommand();
            command.CommandText = "SELECT AgreementId, Agreed FROM Agreements";

            var results = new Dictionary<Agreement, bool>();
            
            await using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync().ConfigureAwait(false))
            {
                var id = (Agreement)reader.GetInt64(0);
                var agreed = reader.GetBoolean(1);
                
                results.Add(id, agreed);
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
    /// <param name="agreement">The agreement</param>
    /// <param name="value">Whether the agreement has been agreed to or not</param>
    public async Task<bool> SetAgreement(Agreement agreement, bool value)
    {
        try
        {
            await using var command = _database.CreateCommand();
            command.CommandText = "INSERT INTO Agreements VALUES (@Agreement, @Value)";
            command.Parameters.AddWithValue("@Agreement", agreement);
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