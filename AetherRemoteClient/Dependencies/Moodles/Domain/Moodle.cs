using System.Text;
using AetherRemoteCommon.Dependencies.Moodles.Domain;

namespace AetherRemoteClient.Dependencies.Moodles.Domain;

/// <summary>
///     Domain representation of a MoodlesStatusInfo from Moodles
/// </summary>
public record Moodle
{
    /// <summary>
    ///     Moodle properties
    /// </summary>
    public MoodleInfo Info = new();
    
    /// <summary>
    ///     A formatting of the <see cref="MoodleInfo.Title"/> field with all colors, accents, and other fields removed
    /// </summary>
    public string PrettyTitle = string.Empty;
    
    /// <summary>
    ///     A formatting of the <see cref="MoodleInfo.Description"/> field with all colors, accents, and other fields removed
    /// </summary>
    public string PrettyDescription = string.Empty;

    /// <summary>
    ///     A formatting of the <see cref="MoodleInfo.ExpireTicks"/> field to display all relevant times
    /// </summary>
    public string PrettyExpiration = string.Empty;
}