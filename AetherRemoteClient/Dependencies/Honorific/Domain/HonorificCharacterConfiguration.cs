using System.Collections.Generic;

namespace AetherRemoteClient.Dependencies.Honorific.Domain;

// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable CollectionNeverUpdated.Global
// ReSharper disable FieldCanBeMadeReadOnly.Global

/// <summary>
///     The model representing Honorific's <see href="https://github.com/Caraxi/Honorific/blob/master/CharacterConfig.cs">CharacterConfig</see>class
/// </summary>
public class HonorificCharacterConfiguration
{
    /// <summary>
    ///     List of all titles created for the character represented by this class instance
    /// </summary>
    public List<HonorificTitle> CustomTitles = [];
}