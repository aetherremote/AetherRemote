namespace AetherRemoteClient.Domain;

public class CharacterConfiguration
{
    // ====================================
    // !! This is a database schema file !!
    // ====================================
    
    public int ConfigurationId;
    public int CharacterId;
    public int Version;
    public bool AutoLogin;
    public string Secret = string.Empty;
}