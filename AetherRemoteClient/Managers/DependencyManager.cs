using System;
using System.Collections.Generic;
using AetherRemoteClient.Domain.Interfaces;
using AetherRemoteClient.Ipc;

namespace AetherRemoteClient.Managers;

/// <summary>
///     Manages dependencies and external plugins to ensure availability
/// </summary>
public class DependencyManager : IDisposable
{
    private readonly List<IExternalPlugin> _dependencies;

    private DateTime _timeLastUpdated = DateTime.Now;
    private double _timeUntilNextProcess = 60000;

    /// <summary>
    ///     <inheritdoc cref="DependencyManager"/>
    /// </summary>
    public DependencyManager(CustomizePlusIpc customize, GlamourerIpc glamourer, MoodlesIpc moodles,
        PenumbraIpc penumbra)
    {
        _dependencies = [customize, glamourer, moodles, penumbra];
        Plugin.PluginInterface.UiBuilder.Draw += Update;
    }

    /// <summary>
    ///     Must be called once every framework update
    /// </summary>
    private void Update()
    {
        var now = DateTime.Now;
        var delta = (now - _timeLastUpdated).TotalMilliseconds;
        _timeLastUpdated = now;

        if (_timeUntilNextProcess > 0)
        {
            _timeUntilNextProcess -= delta;
            return;
        }

        foreach (var plugin in _dependencies)
            plugin.TestIpcAvailability();

        // Every minute
        _timeUntilNextProcess = 60000;
    }

    public void Dispose()
    {
        Plugin.PluginInterface.UiBuilder.Draw -= Update;
        GC.SuppressFinalize(this);
    }
}