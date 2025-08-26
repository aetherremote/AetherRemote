using System;
using System.Collections.Generic;
using Dalamud.Configuration;

namespace AetherRemoteClient.Domain.Configurations;

/// <summary>
///     Obsolete class representing the original configuration files
/// </summary>

[Serializable]
public class LegacyConfiguration : IPluginConfiguration
{
    public int Version { get; set; } = 1;
    public bool AutoLogin = false;
    public bool SafeMode = false;
    public string Secret = string.Empty;
    public Dictionary<string, string> Notes { get; set; } = [];
    public Dictionary<string, PermanentTransformationData> PermanentTransformations { get; set; } = [];
}