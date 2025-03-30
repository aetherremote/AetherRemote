using System;
using System.Collections.Generic;
using AetherRemoteClient.Domain.Interfaces;
using AetherRemoteClient.Ipc;

namespace AetherRemoteClient.Managers;

/// <summary>
///     Manages dependencies and external plugins to ensure availability
/// </summary>
public class DependencyManager(GlamourerIpc glamourer, MoodlesIpc moodles, PenumbraIpc penumbra)
{
    private readonly List<IExternalPlugin> _dependencies = [glamourer, moodles, penumbra];
    
    private DateTime _timeLastUpdated = DateTime.Now;
    private double _timeUntilNextProcess = 60000;

    /// <summary>
    ///     Must be called once every framework update
    /// </summary>
    public void Update()
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
}