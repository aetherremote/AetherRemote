using System;
using System.Collections.Generic;
using AetherRemoteClient.Ipc;
using AetherRemoteCommon.Domain.Enums;

namespace AetherRemoteClient.Domain;

/// <summary>
///     Represents the saved data required for a permanent transformation to be applied when a character logs in
/// </summary>
[Serializable]
public class PermanentTransformationData
{
    /// <summary>
    ///     Glamourer data to be applied
    /// </summary>
    public string GlamourerData { get; set; } = string.Empty;
    
    /// <summary>
    ///     How should this transformation be applied?
    /// </summary>
    public GlamourerApplyFlags GlamourerApplyFlags { get; set; }
    
    /// <summary>
    ///     Mod path data to be applied
    /// </summary>
    public Dictionary<string, string>? ModPathData { get; set; }
    
    /// <summary>
    ///     Mod meta data
    /// </summary>
    public string? ModMetaData { get; set; }
    
    /// <summary>
    ///     Customize Plus data to be applied.
    ///     This should be serialized from the IList directly in the <see cref="CustomizePlusIpc"/> service,
    ///     then reserialized in that same service
    /// </summary>
    public string? CustomizePlusData { get; set; }
    
    /// <summary>
    ///     Moodles data to be applied
    /// </summary>
    public string? MoodlesData { get; set; }
    
    /// <summary>
    ///     The code to unlock (delete) this permanent transformation
    /// </summary>
    public string UnlockCode { get; set; } = string.Empty;
}