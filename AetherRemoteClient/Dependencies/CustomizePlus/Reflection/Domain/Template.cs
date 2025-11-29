namespace AetherRemoteClient.Dependencies.CustomizePlus.Reflection.Domain;

/// <summary>
///     Domain encapsulation of a reflected Customize Plus Template
/// </summary>
public class Template(object template)
{
    /// <summary>
    ///     Customize Plus Template
    /// </summary>
    public readonly object Value = template;
}