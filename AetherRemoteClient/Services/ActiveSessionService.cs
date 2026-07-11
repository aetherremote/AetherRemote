using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AetherRemoteClient.Domain.Enums;
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
    // ======== Character Settings ========
    
    /// <summary> This session's character name </summary>
    public string? CharacterName { get; private set; }
    
    /// <summary> This session's character world </summary>
    public string? CharacterWorld { get; private set; }
    
    /// <summary> This session's character id, which can be used for permanent transformation as of right now </summary>
    public long? CharacterId { get; private set; }
    
    /// <summary> This session's pending secret id, either loaded on initialization, or selected from the ui </summary>
    public long? PendingSecretId { get; private set; }

    /// <summary> Settings loaded along with pending secret id for the case of AutoLogin </summary>
    private Dictionary<SecretSetting, string>? _pendingSettings;
    
    // ======== Online Session ========

    /// <summary> This session's secret id, set AFTER a character has successfully logged in, copied from <see cref="PendingSecretId"/> </summary>
    public long? SecretId { get; private set; }
    
    /// <summary> This session's friend code, as retrieved from the server, set AFTER a character has successfully logged in </summary>
    public string? FriendCode { get; private set; }

    /// <summary> This session's global permissions, as retrieved from the server, set AFTER a character has successfully logged in </summary>
    public ResolvedPermissions? GlobalPermissions { get; private set; }
    
    // ======== Online Session Settings ========
    
    /// <summary> If the client should attempt to log in automatically </summary>
    public bool AutoLogin { get; private set; }
    
    // ======== Events ========
    
    /// <summary> Event fired whenever the global permissions are set or updated </summary>
    public event Action<ResolvedPermissions?>? GlobalPermissionsChanged;
    
    /// <summary>
    ///     Create a new active session, resetting the previous values, and loading configuration values and settings
    /// </summary>
    public async Task<bool> StartNewSession(string characterName, string characterWorld)
    {
        ClearAllSessionData();
        
        if (await databaseInfrastructure.GetCharacterConfiguration(characterName, characterWorld).ConfigureAwait(false) is not { } configuration)
            return false;
        
        CharacterName = configuration.Name;
        CharacterWorld = configuration.World;
        CharacterId = configuration.Id;
        
        // At this point, this is all we can derive from the character, so exit early
        if (configuration.SecretId is not { } secretId)
            return true;
        
        // AutoLogin is unique in that we need to test for this before actually fully loading the configuration
        if (await databaseInfrastructure.GetSecretSettings(secretId).ConfigureAwait(false) is not { } settings)
            return false;
        
        AutoLogin = settings.TryGetValue(SecretSetting.AutoLogin, out var autoLogin) && bool.Parse(autoLogin);

        return UpdatePendingSecretId(secretId, settings);
    }

    /// <summary>
    ///     Update the active session with information retrieved from the server, as well as populate settings
    /// </summary>
    public async Task<bool> UpdateAccountDetails(string friendCode, ResolvedPermissions globalPermissions)
    {
        if (PendingSecretId is not { } pendingSecretId || CharacterId is not { } characterId)
            return false;
        
        if (_pendingSettings is not { } settings)
            return false;

        await databaseInfrastructure.SetCharacterSecretId(characterId, pendingSecretId).ConfigureAwait(false);
        
        // Setting SecretId here signifies we have successfully 'logged in'
        SecretId = pendingSecretId;
        
        // Now we can set all the settings from our provided secret
        AutoLogin = settings.TryGetValue(SecretSetting.AutoLogin, out var autoLogin) && bool.Parse(autoLogin);
        
        FriendCode = friendCode;
        GlobalPermissions = globalPermissions;
        GlobalPermissionsChanged?.Invoke(globalPermissions);
        return true;
    }

    /// <summary>
    ///     Update the pending secret id to be used for logging in, as well as loading that secret id's pending settings
    /// </summary>
    public async Task<bool> UpdatePendingSecretId(long secretId)
    {
        if (await databaseInfrastructure.GetSecretSettings(secretId).ConfigureAwait(false) is not { } settings)
            return false;

        return UpdatePendingSecretId(secretId, settings);
    }
    
    /// <inheritdoc cref="UpdatePendingSecretId"/>
    private bool UpdatePendingSecretId(long secretId, Dictionary<SecretSetting, string>? settings)
    {
        PendingSecretId = secretId;
        _pendingSettings = settings;
        return true;
    }

    /// <summary>
    ///     Update the global permissions to use when resolving things like permissions
    /// </summary>
    public void UpdateGlobalPermissions(ResolvedPermissions globalPermissions)
    {
        GlobalPermissions = globalPermissions;
        GlobalPermissionsChanged?.Invoke(globalPermissions);
    }

    /// <summary>
    ///     Set the AutoLogin setting for the ACTIVE secret id
    /// </summary>
    /// <remarks> The SecretId must be set for this to apply </remarks>
    public async Task<bool> SetAutoLogin(bool autoLogin)
    {
        if (SecretId is null)
            return false;
        
        if (await databaseInfrastructure.SetSecretSetting(SecretId.Value, SecretSetting.AutoLogin, autoLogin).ConfigureAwait(false) is false)
            return false;
        
        AutoLogin = autoLogin;
        return true;
    }

    /// <summary>
    ///     Removes all information about an active offline session (FriendCode, SecretId, etc...)
    /// </summary>
    public void ClearOnlineSessionData()
    {
        SecretId = null;
        AutoLogin = false;
        FriendCode = null;
        GlobalPermissions = null;
    }
    
    /// <summary>
    ///     Removes all information about the local character, and online session
    /// </summary>
    public void ClearAllSessionData()
    {
        CharacterName = null;
        CharacterWorld = null;
        CharacterId = null;
        PendingSecretId = null;
        _pendingSettings = null;
        SecretId = null;
        AutoLogin = false;
        FriendCode = null;
        GlobalPermissions = null;
    }
}