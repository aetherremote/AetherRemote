using System;
using System.Collections.Generic;
using System.Linq;
using System.Timers;
using AetherRemoteClient.Dependencies.CustomizePlus.Services;
using AetherRemoteClient.Dependencies.Glamourer.Services;
using AetherRemoteClient.Dependencies.Honorifics.Services;
using AetherRemoteClient.Dependencies.Moodles.Services;
using AetherRemoteClient.Dependencies.Penumbra.Services;
using AetherRemoteClient.Domain.Interfaces;
using Dalamud.Plugin;

namespace AetherRemoteClient.Managers;

// ReSharper disable RedundantBoolCompare

/// <summary>
///     Manages dependencies and external plugins to ensure availability
/// </summary>
public class DependencyManager : IDisposable
{
    // Const
    private const int PluginTestInternalMilliseconds = 10 * 1000;
    private const int MaxRetries = 4;
    
    /// <summary>
    ///     A map of the plugin's internal name to their corresponding service
    /// </summary>
    private readonly Dictionary<string, IExternalPlugin> _relevantPluginToService;
    
    /// <summary>
    ///     A list of each dependent's corresponding service to validate the readiness of that service
    /// </summary>
    private readonly HashSet<IExternalPlugin> _pluginsToValidateReadiness;
    
    // Instantiated
    private readonly Timer _validatePluginTimer = new(PluginTestInternalMilliseconds);
    
    // How many times the ValidateDependentPlugins can retry
    private int _retryCounter = MaxRetries;

    /// <summary>
    ///     <inheritdoc cref="DependencyManager"/>
    /// </summary>
    public DependencyManager(CustomizePlusService customizePlusService, GlamourerService glamourerService, HonorificService honorificService, MoodlesService moodlesService, PenumbraService penumbraService)
    {
        _relevantPluginToService = [];
        _relevantPluginToService.Add("CustomizePlus", customizePlusService);
        _relevantPluginToService.Add("Glamourer", glamourerService);
        _relevantPluginToService.Add("Honorific", honorificService);
        _relevantPluginToService.Add("Moodles", moodlesService);
        _relevantPluginToService.Add("Penumbra", penumbraService);

        _pluginsToValidateReadiness = [customizePlusService, glamourerService, honorificService, moodlesService, penumbraService];

        _validatePluginTimer.Elapsed += ValidateDependentPlugins;
        _validatePluginTimer.AutoReset = true;
        _validatePluginTimer.Enabled = true;

        Plugin.PluginInterface.ActivePluginsChanged += OnActivePluginsChanged;
    }

    private async void ValidateDependentPlugins(object? sender, ElapsedEventArgs e)
    {
        try
        {
            if (_pluginsToValidateReadiness.Count is 0)
                return;
        
            foreach (var plugin in _pluginsToValidateReadiness.ToList())
                if (await plugin.TestIpcAvailability().ConfigureAwait(false))
                    _pluginsToValidateReadiness.Remove(plugin);
            
            if (_retryCounter is 0)
                _pluginsToValidateReadiness.Clear();

            _retryCounter--;
        }
        catch (Exception)
        {
            // Ignore
        }
    }
    
    private void OnActivePluginsChanged(IActivePluginsChangedEventArgs args)
    {
        foreach (var name in args.AffectedInternalNames)
        {
            if (_relevantPluginToService.TryGetValue(name, out var plugin) is false)
                continue;
        
            switch (args.Kind)
            {
                case PluginListInvalidationKind.Loaded:
                case PluginListInvalidationKind.Update:
                case PluginListInvalidationKind.AutoUpdate:
                    _retryCounter = MaxRetries;
                    _pluginsToValidateReadiness.Add(plugin);
                    break;
            
                case PluginListInvalidationKind.Unloaded:
                    _retryCounter = MaxRetries;
                    _pluginsToValidateReadiness.Remove(plugin);
                    break;
            
                default:
                    continue;
            }
        }
    }

    public void Dispose()
    {
        _validatePluginTimer.Elapsed -= ValidateDependentPlugins;
        _validatePluginTimer.Dispose();
        GC.SuppressFinalize(this);
    }
}