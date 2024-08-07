using Dalamud.Plugin.Services;
using System;
using System.Collections.Generic;

namespace AetherRemoteClient.Domain.Logger;

public class AetherRemoteLogger
{
    public List<AetherRemoteInternalLog> InternalLogs { get; private set; } = [];

    private readonly IPluginLog logger;

    public AetherRemoteLogger(IPluginLog logger)
    {
        this.logger = logger;
    }

    // Wrappers
    public void Verbose(string message) => logger.Verbose(message);
    public void Debug(string message) => logger.Debug(message);
    public void Information(string message) => logger.Information(message);
    public void Warning(string message) => logger.Warning(message);
    public void Error(string message) => logger.Error(message);
    public void Fatal(string message) => logger.Fatal(message);


    /// <summary>
    /// Log a message to the internal Aether Remote log tab.
    /// </summary>
    public void LogInternal(string message)
    {
        LogInternal(new AetherRemoteInternalLog(message, DateTime.Now));
    }

    /// <summary>
    /// Log a message to the internal Aether Remote log tab.
    /// </summary>
    public void LogInternal(AetherRemoteInternalLog log)
    {
        InternalLogs.Add(log);
    }
}
