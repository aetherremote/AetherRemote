namespace AetherRemoteClient.Dependencies.CustomizePlus.Reflection.Domain;

/// <summary>
///     Domain encapsulation of a reflected Customize Plus Profile
/// </summary>
public class Profile(object profile)
{
    /// <summary>
    ///     Customize Plus Profile
    /// </summary>
    public readonly object Value = profile;
}