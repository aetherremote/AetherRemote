using System.Numerics;
using AetherRemoteCommon.Dependencies.Honorific.Domain;

namespace AetherRemoteClient.Dependencies.Honorific.Domain;

// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable CollectionNeverUpdated.Global
// ReSharper disable FieldCanBeMadeReadOnly.Global
// ReSharper disable UnassignedField.Global

/// <summary>
///     The model representing Honorific's <see href="https://github.com/Caraxi/Honorific/blob/master/CustomTitle.cs">CustomTitle</see>class
/// </summary>
public class HonorificTitle
{
    /// <summary>
    ///     The title of this honorific
    /// </summary>
    public string? Title = string.Empty;

    /// <summary>
    ///     If this honorific is a prefix or not
    /// </summary>
    public bool IsPrefix;
    
    /// <summary>
    ///     The color of this honorific, if one exists
    /// </summary>
    public Vector3? Color;
    
    /// <summary>
    ///     The color glow of this honorific, if one exists
    /// </summary>
    public Vector3? Glow;

    /// <summary>
    ///     Converts this object into its corresponding <see cref="HonorificInfo"/>
    /// </summary>
    /// <returns></returns>
    public HonorificInfo ToHonorificInfo()
    {
        return new HonorificInfo
        {
            Title = Title,
            IsPrefix = IsPrefix,
            Color = Color,
            Glow = Glow
        };
    }
}