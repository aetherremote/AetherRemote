using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AetherRemoteClient.Dependencies.CustomizePlus.Domain;
using AetherRemoteClient.Dependencies.CustomizePlus.Reflection;
using AetherRemoteClient.Domain;
using AetherRemoteClient.Domain.Interfaces;
using Dalamud.Plugin.Ipc;
using ProfileData = (
    System.Guid Id,
    string Name,
    string Path,
    System.Collections.Generic.List<(string Name, ushort World, byte Type, ushort SubType)>,
    int Priority,
    bool Enabled);

namespace AetherRemoteClient.Dependencies.CustomizePlus.Services;

// ReSharper disable RedundantBoolCompare

/// <summary>
///     Provides access to CustomizePlus
/// </summary>
public class CustomizePlusService : IExternalPlugin
{
    // Default folder for designs without homes
    private const string Uncategorized = "Uncategorized";
    
    // Const
    private const int ExpectedMajor = 6;
    private const int ExpectedMinor = 4;
    
    // Instantiated
    private readonly ICallGateSubscriber<(int, int)> _getVersion;
    private readonly ICallGateSubscriber<IList<ProfileData>> _getProfileList;
    private readonly ICallGateSubscriber<Guid, (int, string?)> _getProfileById;
    
    /// <summary>
    ///     Reflected instance of CustomizePlus
    /// </summary>
    private CustomizePlusPlugin? _customizePlusPlugin;

    /// <summary>
    ///     Is CustomizePlus available for use?
    /// </summary>
    public bool ApiAvailable;
    
    /// <summary>
    ///     <inheritdoc cref="CustomizePlusService"/>
    /// </summary>
    public CustomizePlusService()
    {
        _getVersion = Plugin.PluginInterface.GetIpcSubscriber<(int, int)>("CustomizePlus.General.GetApiVersion");
        _getProfileList = Plugin.PluginInterface.GetIpcSubscriber<IList<ProfileData>>("CustomizePlus.Profile.GetList");
        _getProfileById = Plugin.PluginInterface.GetIpcSubscriber<Guid, (int, string?)>("CustomizePlus.Profile.GetByUniqueId");
    }
    
    /// <summary>
    ///     Tests for availability to CustomizePlus
    /// </summary>
    public async Task<bool> TestIpcAvailability()
    {
        // Set everything to disabled state
        _customizePlusPlugin = null;
        ApiAvailable = false;
        
        // Invoke Api
        var version = await Plugin.RunOnFrameworkSafely(() => _getVersion.InvokeFunc()).ConfigureAwait(false);
        
        // Test for proper versioning
        if (version.Item1 is not ExpectedMajor || version.Item2 < ExpectedMinor)
            return false;

        // Check to make sure the reflection process was successful
        if (await Plugin.RunOnFrameworkSafely(CustomizePlusPlugin.Create).ConfigureAwait(false) is not { } plugin)
            return false;
        
        // Call the delete method for safety
        await DeleteTemporaryCustomizeAsync().ConfigureAwait(false);
        
        // Mark as ready
        _customizePlusPlugin = plugin;
        ApiAvailable = true;
        return true;
    }

    /// <summary>
    ///     Gets a list of all the customize plus profile identifiers
    /// </summary>
    public async Task<List<Folder<Profile>>> GetProfiles()
    {
        try
        {
            if (ApiAvailable is false)
                return [];
            
            var result = await Plugin.RunOnFramework(() => _getProfileList.InvokeFunc()).ConfigureAwait(false);

            var folders = new Dictionary<string, List<Profile>>();
            foreach (var data in result)
            {
                var profile = new Profile(data.Id, data.Name, data.Path);
                var span = data.Path.AsSpan();
                var index = span.LastIndexOf('/');

                var folderPathSpan = index is -1
                    ? Uncategorized.AsSpan()
                    : span[..index];

                var folderPath = folderPathSpan.ToString();

                if (folders.TryGetValue(folderPath, out var list))
                {
                    list.Add(profile);
                }
                else
                {
                    folders[folderPath] = [profile];
                }
            }

            foreach (var list in folders.Values)
                list.Sort((x, y) => string.Compare(x.Name, y.Name, StringComparison.OrdinalIgnoreCase));

            return folders
                .Select(x => new Folder<Profile>(x.Key, x.Value))
                .OrderBy(x => x.Path, StringComparer.OrdinalIgnoreCase)
                .ThenBy(x => x.Path.Equals(Uncategorized, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }
        catch (Exception e)
        {
            Plugin.Log.Warning($"[CustomizePlusService.GetProfiles] An error occurred, {e}");
            return [];
        }
    }

    /// <summary>
    ///     Gets a customize plus profile by guid
    /// </summary>
    public async Task<string?> GetProfile(Guid guid)
    {
        try
        {
            if (ApiAvailable is false)
                return null;
            
            var tuple = await Plugin.RunOnFramework(() => _getProfileById.InvokeFunc(guid)).ConfigureAwait(false);
            return tuple.Item1 is 0 ? tuple.Item2 : null;
        }
        catch (Exception e)
        {
            Plugin.Log.Warning($"[CustomizePlusService.GetProfile] An error occurred, {e}");
            return null;
        }
    }

    /// <summary>
    ///     Apply a CustomizePlus profile to the local player
    /// </summary>
    /// <param name="json">Profile data, most commonly retrieved from <see cref="GetProfile"/> or via the "Copy Template" button in CustomizePlus UI</param>
    public async Task<bool> ApplyCustomizeAsync(string json)
    {
        if (ApiAvailable is false)
            return false;
        
        return await Task.Run(() => ApplyCustomizeOnFrameworkAsync(json)).ConfigureAwait(false);
    }

    /// <summary>
    ///     <inheritdoc cref="ApplyCustomizeAsync"/>
    /// </summary>
    private async Task<bool> ApplyCustomizeOnFrameworkAsync(string json)
    {
        return await Plugin.RunOnFramework(() =>
        {
            try
            {
                if (_customizePlusPlugin?.TemplateManager.DeserializeTemplate(json) is not { } template)
                    return false;
            
                if (_customizePlusPlugin.ProfileManager.DeleteTemporaryProfile() is false)
                    return false;

                if (_customizePlusPlugin.ProfileManager.CreateProfile() is not { } profile)
                    return false;
            
                if (_customizePlusPlugin.ProfileManager.AddCharacter(profile) is false)
                    return false;

                if (_customizePlusPlugin.ProfileManager.AddTemplate(profile, template) is false)
                    return false;

                if (_customizePlusPlugin.ProfileManager.SetPriority(profile) is false)
                    return false;

                if (_customizePlusPlugin.ProfileManager.SetEnabled(profile) is false)
                    return false;
                
                return true;
            }
            catch (Exception e)
            {
                Plugin.Log.Warning($"[CustomizePlusService.ApplyProfileByJson] An error occurred, {e}");
                return false;
            }
        }).ConfigureAwait(false);
    }
    
    /// <summary>
    ///     Deletes the temporary profile created by Aether Remote, if one exists
    /// </summary>
    public async Task<bool> DeleteTemporaryCustomizeAsync()
    {
        if (ApiAvailable is false)
            return false;
        
        return await Task.Run(DeleteTemporaryCustomizeOnFrameworkAsync).ConfigureAwait(false);
    }

    /// <summary>
    ///     <inheritdoc cref="DeleteTemporaryCustomizeAsync"/>
    /// </summary>
    private async Task<bool> DeleteTemporaryCustomizeOnFrameworkAsync()
    {
        return await Plugin.RunOnFramework(() => _customizePlusPlugin?.ProfileManager.DeleteTemporaryProfile() ?? false).ConfigureAwait(false);
    }
}