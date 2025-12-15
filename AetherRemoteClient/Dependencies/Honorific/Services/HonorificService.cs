using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AetherRemoteClient.Dependencies.Honorific.Domain;
using AetherRemoteClient.Domain.Interfaces;
using AetherRemoteCommon.Dependencies.Honorific.Domain;
using Dalamud.Plugin.Ipc;
using Newtonsoft.Json;

namespace AetherRemoteClient.Dependencies.Honorific.Services;

// ReSharper disable RedundantBoolCompare

/// <summary>
///     Provides access to Honorific
/// </summary>
public class HonorificService : IExternalPlugin
{
    // Const
    private const int ExpectedMajor = 3;
    private const int ExpectedMinor = 2;
    private static readonly JsonSerializerSettings Options = new() { TypeNameHandling = TypeNameHandling.None };
    
    // Instantiated
    private readonly ICallGateSubscriber<(uint, uint)> _apiVersion;
    private readonly ICallGateSubscriber<int, object> _clearCharacterTitle;
    private readonly ICallGateSubscriber<int, string> _getCharacterTitle;
    private readonly ICallGateSubscriber<string> _getLocalCharacterTitle;
    private readonly ICallGateSubscriber<int, string, object> _setCharacterTitle;
    
    /// <summary>
    ///     Is Honorific available for use?
    /// </summary>
    public bool ApiAvailable;
    
    /// <summary>
    ///     <inheritdoc cref="IExternalPlugin.IpcReady"/>
    /// </summary>
    public event EventHandler? IpcReady;

    /// <summary>
    ///     <inheritdoc cref="HonorificService"/>
    /// </summary>
    public HonorificService()
    {
        _apiVersion = Plugin.PluginInterface.GetIpcSubscriber<(uint, uint)>("Honorific.ApiVersion");
        _getCharacterTitle = Plugin.PluginInterface.GetIpcSubscriber<int, string>("Honorific.GetCharacterTitle");
        _getLocalCharacterTitle = Plugin.PluginInterface.GetIpcSubscriber<string>("Honorific.GetLocalCharacterTitle");
        _clearCharacterTitle = Plugin.PluginInterface.GetIpcSubscriber<int, object>("Honorific.ClearCharacterTitle");
        _setCharacterTitle = Plugin.PluginInterface.GetIpcSubscriber<int, string, object>("Honorific.SetCharacterTitle");
    }
    
    /// <summary>
    ///     Tests for availability to Honorific
    /// </summary>
    public async Task<bool> TestIpcAvailability()
    {
        ApiAvailable = false;

        var (major, minor) = await Plugin.RunOnFrameworkSafely(() => _apiVersion.InvokeFunc()).ConfigureAwait(false);

        if (major is not ExpectedMajor || minor < ExpectedMinor)
            return false;
        
        ApiAvailable = true;
        
        IpcReady?.Invoke(this, EventArgs.Empty);
        return true;
    }
    
    /// <summary>
    ///     Clears the local character's title. Honorific does not allow clients to change their honorifics if set by another plugin
    /// </summary>
    public async Task<bool> ClearCharacterTitle(int index)
    {
        try
        {
            await Plugin.RunOnFramework(() => _clearCharacterTitle.InvokeAction(index)).ConfigureAwait(false);
            return true;
        }
        catch (Exception e)
        {
            Plugin.Log.Error($"[HonorificService.ClearTitle] {e}");
            return false;
        }
    }

    /// <summary>
    ///     Gets any character's title as JSON
    /// </summary>
    public async Task<HonorificInfo?> GetCharacterTitle(int characterObjectIndex)
    {
        try
        {
            var json = await Plugin.RunOnFramework(() => _getCharacterTitle.InvokeFunc(characterObjectIndex)).ConfigureAwait(false);
            return JsonConvert.DeserializeObject<HonorificInfo>(json);
        }
        catch (Exception e)
        {
            Plugin.Log.Error($"[HonorificService.GetCharacterTitle] {e}");
            return null;
        }
    }

    /// <summary>
    ///     Gets all the characters with titles created.
    /// </summary>
    /// <returns>A dictionary mapping world id to a dictionary mapping character name to a list of titles</returns>
    public static async Task<Dictionary<uint, Dictionary<string, List<HonorificInfo>>>> GetCharacterTitleList()
    {
        // NOTE:    This is probably not an ideal solution. In order to get ALL the honorifics for every character
        //          it makes more sense to load the actual configuration and get the required data, rather than
        //          calling the IPC to get just the current character's.
        
        try
        {
            // Go up a level so we're in just the configuration directory
            if (Plugin.PluginInterface.ConfigDirectory.Parent is not { } parent)
                return [];
            
            // Construct the path to the file and load the configuration
            var path = Path.Combine(parent.FullName, "Honorific.json");
            var raw = await File.ReadAllTextAsync(path).ConfigureAwait(false);

            // Convert the JSON into our local domain model of all the fields relevant
            if (JsonConvert.DeserializeObject<HonorificConfiguration>(raw, Options) is not { } configuration)
                return [];
            
            // Convert the returned object to domain models
            var results = new Dictionary<uint, Dictionary<string, List<HonorificInfo>>>();
            foreach (var (world, dictionary) in configuration.WorldCharacterDictionary)
            {
                var sub = new Dictionary<string, List<HonorificInfo>>();
                foreach (var (character, config) in dictionary)
                    sub[character] = config.CustomTitles.Select(title => title.ToHonorificInfo()).ToList();
                
                results.Add(world, sub);
            }
            
            return results;
        }
        catch (Exception e)
        {
            Plugin.Log.Error($"[HonorificService.GetCharacterTitleList] {e}");
            return [];
        }
    }

    /// <summary>
    ///     Gets the local player's title
    /// </summary>
    public async Task<string?> GetLocalCharacterTitle()
    {
        try
        {
            return await Plugin.RunOnFramework(() => _getLocalCharacterTitle.InvokeFunc()).ConfigureAwait(false);
        }
        catch (Exception e)
        {
            Plugin.Log.Error($"[HonorificService.GetLocalCharacterTitle] {e}");
            return null;
        }
    }
    
    /// <summary>
    ///     Sets a title
    /// </summary>
    public async Task<bool> SetCharacterTitle(int index, HonorificInfo title)
    {
        try
        {
            var json = JsonConvert.SerializeObject(title);
            await Plugin.RunOnFramework(() => _setCharacterTitle.InvokeAction(index, json)).ConfigureAwait(false);
            return true;
        }
        catch (Exception e)
        {
            Plugin.Log.Error($"[HonorificService.SetTitle] {e}");
            return false;
        }
    }
}