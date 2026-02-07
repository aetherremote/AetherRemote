using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using AetherRemoteClient.Dependencies.Honorific.Domain;
using AetherRemoteClient.Domain.Interfaces;
using AetherRemoteCommon.Dependencies.Honorific.Domain;
using Dalamud.Plugin.Ipc;
using Newtonsoft.Json;

namespace AetherRemoteClient.Dependencies.Honorific.Services;

/// <summary>
///     Provides access to Honorific
/// </summary>
public class HonorificService : IExternalPlugin
{
    /* =========================================================================
     *                               READ ME
     * =========================================================================
     * There are issues with syncing services and applying
     * temporary honorifics to the local player, causing crashes on re-loading.
     * To get around this, some of the IPCs have been replaced with
     * functionality to call using the command handler.
     */
    
    // Const
    private const int ExpectedMajor = 3;
    private const int ExpectedMinor = 2;
    private static readonly JsonSerializerSettings Options = new() { TypeNameHandling = TypeNameHandling.None };
    
    // Instantiated
    private readonly ICallGateSubscriber<(uint, uint)> _apiVersion;
    private readonly ICallGateSubscriber<int, string> _getCharacterTitle;
    
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
    }
    
    /// <summary>
    ///     Tests for availability to Honorific
    /// </summary>
    public async Task<bool> TestIpcAvailability()
    {
        ApiAvailable = false;
        
        try
        {
            var (major, minor) = await Plugin.RunOnFramework(() => _apiVersion.InvokeFunc()).ConfigureAwait(false);
            if (major is not ExpectedMajor || minor < ExpectedMinor)
                return false;
        
            ApiAvailable = true;
        
            IpcReady?.Invoke(this, EventArgs.Empty);
            return true;
        }
        catch (Exception e)
        {
            Plugin.Log.Error($"[HonorificService.TestIpcAvailability] {e}");
            return false;
        }
    }
    
    /// <summary>
    ///     Clears the local character's title. Honorific does not allow clients to change their honorifics if set by another plugin
    /// </summary>
    public bool ClearCharacterTitle()
    {
        return Plugin.CommandManager.ProcessCommand("/honorific force clear");
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
    ///     Sets a title
    /// </summary>
    public async Task<bool> SetCharacterTitle(HonorificInfo honorific)
    {
        var sb = new StringBuilder();
        sb.Append("/honorific force set ");
        sb.Append(honorific.Title);
        sb.Append(" | ");
        sb.Append(honorific.IsPrefix ? "prefix" : "suffix");

        if (honorific.Color is not null)
        {
            sb.Append(" | ");
            sb.Append(ToHex(honorific.Color));
        }

        if (honorific.Glow is not null)
        {
            // The command function doesn't let us skip syntax, so we need to include a generic white
            if (honorific.Color is null)
            {
                sb.Append(" | ");
                sb.Append("#FFFFFF");
            }
            
            sb.Append(" | ");
            sb.Append(ToHex(honorific.Glow));
        }

        return await Plugin.RunOnFramework(() => Plugin.CommandManager.ProcessCommand(sb.ToString())).ConfigureAwait(false);
    }
    
    private static string ToHex(Vector3 color)
    {
        var r = (byte)Math.Clamp(color.X * byte.MaxValue, byte.MinValue, byte.MaxValue);
        var g = (byte)Math.Clamp(color.Y * byte.MaxValue, byte.MinValue, byte.MaxValue);
        var b = (byte)Math.Clamp(color.Z * byte.MaxValue, byte.MinValue, byte.MaxValue);
        return $"#{r:X2}{g:X2}{b:X2}";
    }
}