using System;
using AetherRemoteCommon.Domain;

namespace AetherRemoteClient.Domain.Hypnosis;

/// <summary>
///     A saved configuration for hypnosis spirals and text
/// </summary>

[Serializable]
public class HypnosisProfile
{
    /// <summary>
    ///     Version, only used when loading to transition to newer versions
    /// </summary>
    public int Version = 1;
    
    /// <summary>
    ///     The name of the profile
    /// </summary>
    public string Name = string.Empty;
    
    /// <summary>
    ///     The actual data the profile contains
    /// </summary>
    public HypnosisData Data = new();
}