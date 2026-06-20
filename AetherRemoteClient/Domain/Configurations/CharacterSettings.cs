namespace AetherRemoteClient.Domain.Configurations;

/// <summary> Settings for a particular secret id </summary>
public class CharacterSettings(
    long secretId,
    bool autoLogin, 
    bool safeMode, 
    bool showDtrBar)
{
    public readonly long SecretId = secretId;
    
    public bool AutoLogin = autoLogin;

    public bool SafeMode = safeMode;

    public bool ShowDtrBar = showDtrBar;
}