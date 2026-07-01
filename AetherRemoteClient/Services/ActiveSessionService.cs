using System.Threading.Tasks;
using AetherRemoteClient.Infrastructure.Database;
using AetherRemoteCommon.Domain;

namespace AetherRemoteClient.Services;

/// <summary>
///     Contains information about a current session. A session is defined as the current 'state' of the variables
///     in use, or used by, parts of the plugin. This includes things such as character name and world, secret id,
///     friend code, and more.
/// </summary>
public class ActiveSessionService(DatabaseInfrastructure databaseInfrastructure)
{
    public long? CharacterId { get; private set; }
    
    public long? SecretId { get; private set; }
    
    public string? FriendCode { get; private set; }

    public ResolvedPermissions? GlobalPermissions { get; private set; }

    /// <summary>
    ///     Initialize character data by loading character id and secret id from the database
    /// </summary>
    public async Task InitializeCharacter(string characterName, string characterWorld)
    {
        if (await databaseInfrastructure.GetCharacterConfiguration(characterName, characterWorld).ConfigureAwait(false) is not { } configuration)
            return;

        SecretId = configuration.SecretId;
        CharacterId = configuration.Id;
    }

    public void SetFriendCode(string friendCode) => FriendCode = friendCode;

    public void SetGlobalPermissions(ResolvedPermissions globalPermissions) => GlobalPermissions = globalPermissions;

    public async Task SetSecretId(long secretId)
    {
        if (CharacterId is null || SecretId is null)
            return;
        
        if (await databaseInfrastructure.SetCharacterSecretId(CharacterId.Value, SecretId.Value).ConfigureAwait(false) is false)
            return;
        
        SecretId = secretId;
    }
}