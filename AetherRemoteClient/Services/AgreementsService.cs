namespace AetherRemoteClient.Services;

/// <summary>
///     Provides a thin layer for retrieving agreements from the client configuration
/// </summary>
public static class AgreementsService
{
    /// <summary>
    ///     A bunch of 
    /// </summary>
    public static class Agreements
    {
        public const string Possession = "possession";
        public const string MoodlesWarning = "moodles-warning";
    }
    
    /// <summary>
    ///     Test if the client has agreed to an agreement
    /// </summary>
    public static bool HasAgreedTo(string agreement)
    {
        if (Plugin.Configuration.Agreements.TryGetValue(agreement, out var agreed))
            return agreed;
        
        Plugin.Configuration.Agreements[agreement] = false;
        _ = Plugin.Configuration.Save();
        return false;
    }

    /// <summary>
    ///     Agree to an agreement
    /// </summary>
    public static void AgreeTo(string agreement)
    {
        Plugin.Configuration.Agreements[agreement] = true;
        _ = Plugin.Configuration.Save();
    }
}