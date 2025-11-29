using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AetherRemoteClient.Dependencies.CustomizePlus.Domain;
using AetherRemoteClient.Dependencies.CustomizePlus.Reflection;
using AetherRemoteClient.Domain.Interfaces;
using Dalamud.Plugin;
using Dalamud.Plugin.Ipc;
using Dalamud.Plugin.Ipc.Exceptions;
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
        Plugin.PluginInterface.ActivePluginsChanged += OnActivePluginsChanged;
        
        _getVersion = Plugin.PluginInterface.GetIpcSubscriber<(int, int)>("CustomizePlus.General.GetApiVersion");
        _getProfileList = Plugin.PluginInterface.GetIpcSubscriber<IList<ProfileData>>("CustomizePlus.Profile.GetList");
        _getProfileById = Plugin.PluginInterface.GetIpcSubscriber<Guid, (int, string?)>("CustomizePlus.Profile.GetByUniqueId");
        
        TestIpcAvailability();
    }
    
    /// <summary>
    ///     Tests for availability to CustomizePlus
    /// </summary>
    public void TestIpcAvailability()
    {
        try
        {
            var version = _getVersion.InvokeFunc();
            if (version.Item1 is ExpectedMajor && version.Item2 >= ExpectedMinor)
            {
                // Try to create a reflected version of the plugin
                if (CustomizePlusPlugin.Create() is { } customizePlusPlugin)
                {
                    ApiAvailable = true;
                    _customizePlusPlugin = customizePlusPlugin;
                }
                else
                {
                    ApiAvailable = false;
                    _customizePlusPlugin = null;
                }
            }
            else
            {
                Plugin.Log.Warning("[CustomizePlusService.TestIpcAvailability] Outdated CustomizePlus version");
                ApiAvailable = false;
            }
        }
        catch (IpcNotReadyError)
        {
            ApiAvailable = false;
        }
        catch (Exception e)
        {
            Plugin.Log.Warning($"[CustomizePlusService.TestIpcAvailability] An error occurred, {e}");
            ApiAvailable = false;
        }
    }

    /// <summary>
    ///     Gets a list of all the customize plus profile identifiers
    /// </summary>
    public async Task<List<Profile>> GetProfiles()
    {
        try
        {
            if (ApiAvailable is false)
                return [];
            
            var tuple = await Plugin.RunOnFramework(() => _getProfileList.InvokeFunc()).ConfigureAwait(false);
            return tuple.Select(profile => new Profile(profile.Id, profile.Name, profile.Path)).ToList();
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

    private void OnActivePluginsChanged(IActivePluginsChangedEventArgs args)
    {
        if (args.AffectedInternalNames.Contains("CustomizePlus"))
            TestIpcAvailability();
    }

    public void Dispose()
    {
        Plugin.PluginInterface.ActivePluginsChanged -= OnActivePluginsChanged;
        GC.SuppressFinalize(this);
    }
}