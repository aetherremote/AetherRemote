using System.Collections.Generic;

namespace AetherRemoteClient.Dependencies.Honorific.Domain;

// ReSharper disable CollectionNeverUpdated.Global
// ReSharper disable FieldCanBeMadeReadOnly.Global

/// <summary>
///     The model representing Honorific's <see href="https://github.com/Caraxi/Honorific/blob/master/PluginConfig.cs">PluginConfig</see>class
/// </summary>
public class HonorificConfiguration
{
    /// <summary>
    ///     Dictionary mapping world id to a Dictionary mapping character name to Honorific Titles
    /// </summary>
    public Dictionary<uint, Dictionary<string, HonorificCharacterConfiguration>> WorldCharacterDictionary = [];
}