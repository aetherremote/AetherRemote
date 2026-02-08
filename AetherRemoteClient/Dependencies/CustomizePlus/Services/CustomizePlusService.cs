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
public class CustomizePlusService : IDisposable, IExternalPlugin
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
    ///     <inheritdoc cref="IExternalPlugin.IpcReady"/>
    /// </summary>
    public event EventHandler? IpcReady;
    
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
        
        // As a safety precaution, attempt to delete any lingering Aether Remote profiles that may exist
        await DeleteTemporaryCustomizeAsync().ConfigureAwait(false);
        
        IpcReady?.Invoke(this, EventArgs.Empty);
        return true;
    }

    /// <summary>
    ///     Gets a list of all the customize plus profile identifiers
    /// </summary>
    public async Task<FolderNode<Profile>?> GetProfiles()
    {
        if (ApiAvailable is false)
            return null;
        
        var result = await Plugin.RunOnFramework(() => _getProfileList.InvokeFunc()).ConfigureAwait(false);
        var root = new FolderNode<Profile>("Root", null);
        foreach (var path in result)
        {
            var parts = path.Path.Split('/', StringSplitOptions.RemoveEmptyEntries);
            var current = root;

            for (var i = 0; i < parts.Length; i++)
            {
                var part = parts[i];
                if (current.Children.TryGetValue(part, out var node) is false)
                {
                    var design = i == parts.Length - 1
                        ? new Profile(path.Id, path.Name)
                        : null;
                    
                    node = new FolderNode<Profile>(part, design);
                    current.Children[part] = node;
                }
                
                current = node;
            }
        }

        SortTree(root);
        
        return root;
    }
    
    /// <summary>
    ///     The dictionary returned by glamourer is not sorted, so we will recursively go through and sort the children
    /// </summary>
    private static void SortTree<T>(FolderNode<T> root)
    {
        // Copy all the children from this node and sort them by folder, then name
        var sorted = root.Children.Values
            .OrderByDescending(node => node.IsFolder)
            .ThenBy(node => node.Name, StringComparer.OrdinalIgnoreCase)
            .ToList();
        
        // Clear all the children with the values sorted and copied
        root.Children.Clear();

        // Reintroduce because dictionaries preserve insertion order
        foreach (var node in sorted)
            root.Children[node.Name] = node;
        
        // Recursively sort the remaining children
        foreach (var child in root.Children.Values)
            SortTree(child);
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
    ///     Tries to get the template data for the active profile on a provided character
    /// </summary>
    /// <returns>The JSON template data, same as if called via GetProfileIpc</returns>
    public async Task<string?> TryGetActiveProfileOnCharacter(string characterNameToSearchFor)
    {
        return await Plugin.RunOnFrameworkSafely(() => _customizePlusPlugin?.ProfileManager.TryGetActiveProfileOnCharacter(characterNameToSearchFor)).ConfigureAwait(false);
    } 

    /// <summary>
    ///     Apply a CustomizePlus profile to the local player
    /// </summary>
    /// <param name="templateJson">Profile data, most commonly retrieved from <see cref="GetProfile"/> or via the "Copy Template" button in CustomizePlus UI</param>
    public async Task<bool> ApplyCustomizeAsync(string? templateJson = null)
    {
        if (ApiAvailable is false)
            return false;

        if (_customizePlusPlugin is null)
            return false;

        return await Plugin.RunOnFramework(() =>
        {
            try
            {
                // Delete previous profile, then create and enable a blank one
                if (_customizePlusPlugin.ProfileManager.DeleteTemporaryProfile() is false) return false;
                if (_customizePlusPlugin.ProfileManager.CreateProfile() is not { } profile) return false;
                if (_customizePlusPlugin.ProfileManager.AddCharacter(profile) is false) return false;
                if (_customizePlusPlugin.ProfileManager.SetPriority(profile) is false) return false;
                if (_customizePlusPlugin.ProfileManager.SetEnabled(profile) is false) return false;

                // If template data was not provided, end early
                if (templateJson is null)
                    return true;

                // Add the template data
                if (_customizePlusPlugin.TemplateManager.DeserializeTemplate(templateJson) is not { } template) return false;
                if (_customizePlusPlugin.ProfileManager.AddTemplate(profile, template) is false) return false;
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

        return await Plugin.RunOnFrameworkSafely(() => _customizePlusPlugin?.ProfileManager.DeleteTemporaryProfile() ?? false).ConfigureAwait(false);
    }

    public async void Dispose()
    {
        try
        {
            await DeleteTemporaryCustomizeAsync().ConfigureAwait(false);
            GC.SuppressFinalize(this);
        }
        catch (Exception)
        {
            GC.SuppressFinalize(this);
        }
    }
}