using System;

namespace AetherRemoteClient.Domain;

/// <summary>
///     Represents a single internal aether remote action log
/// </summary>
public class InternalLog
{
    /// <summary>
    ///     When the log was created
    /// </summary>
    public DateTime TimeStamp;

    /// <summary>
    ///     What is the content of the log?
    /// </summary>
    public string Message = string.Empty;
}