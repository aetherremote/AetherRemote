using System.Collections.Generic;
using AetherRemoteClient.Domain.Glamourer;
using AetherRemoteCommon.Domain.Enums;

namespace AetherRemoteClient.Domain;

/// <summary>
///     Represents the saved data required for a permanent transformation to be applied when a character logs in
/// </summary>
public class PermanentTransformationData
{
    // ====================================
    // !! This is a database schema file !!
    // ====================================
    
    // public int CharacterId;
    // public int Version;
    public string Sender = string.Empty;
    public GlamourerDesign GlamourerDesign = new();
    public GlamourerApplyFlags GlamourerApplyType;
    public string Key = string.Empty;
    public Dictionary<string, string>? ModPathData;
    public string? ModMetaData;
    public string? CustomizePlusData;
    public string? MoodlesData;
}