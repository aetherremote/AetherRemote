using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AetherRemoteClient.Domain.CustomizePlus;
using AetherRemoteClient.Domain.CustomizePlus.Reflection;
using AetherRemoteClient.Domain.CustomizePlus.Reflection.Domain;
using AetherRemoteClient.Domain.Interfaces;
using AetherRemoteClient.Utils;
using Dalamud.Plugin.Ipc;
using Dalamud.Plugin.Ipc.Exceptions;
using ProfileData = (
    System.Guid Id,
    string Name,
    string Path,
    System.Collections.Generic.List<(string Name, ushort World, byte Type, ushort SubType)>,
    int Priority,
    bool Enabled);

namespace AetherRemoteClient.Services;

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

        try
        {
            // Invoke Api
            var version = await DalamudUtilities.RunOnFramework(() => _getVersion.InvokeFunc()).ConfigureAwait(false);

            // Test for proper versioning
            if (version.Item1 is not ExpectedMajor || version.Item2 < ExpectedMinor)
                return false;

            // Check to make sure the reflection process was successful
            if (await DalamudUtilities.RunOnFramework(CustomizePlusPlugin.Create).ConfigureAwait(false) is not { } plugin)
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
        catch (IpcNotReadyError)
        {
            // Exit gracefully
            return false;
        }
        catch (Exception e)
        {
            Plugin.Log.Error($"[CustomizePlusService.TestIpcAvailability] {e}");
            return false;
        }
    }

    /// <summary>
    ///     Gets a list of all customize plus profiles
    /// </summary>
    /// <returns></returns>
    public async Task<List<Profile>> GetProfilesPlain()
    {
        var result = await DalamudUtilities.RunOnFramework(() => _getProfileList.InvokeFunc()).ConfigureAwait(false);
        var profiles = new List<Profile>();
        foreach (var profile in result)
            profiles.Add(new Profile(profile.Id, profile.Name, profile.Path));
        
        return profiles;
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
            
            var tuple = await DalamudUtilities.RunOnFramework(() => _getProfileById.InvokeFunc(guid)).ConfigureAwait(false);
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
    public async Task<string?> TryGetActiveProfileOnCharacter(string characterName, string characterWorld)
    {
        try
        {
            return await DalamudUtilities.RunOnFramework(() => _customizePlusPlugin?.ProfileManager.TryGetActiveIpcProfileOnCharacter(characterName, characterWorld)).ConfigureAwait(false);
        }
        catch (Exception e)
        {
            Plugin.Log.Error($"[CustomizePlusService.TryGetActiveProfileOnCharacter] {e}");
            return null;
        }
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

        return await DalamudUtilities.RunOnFramework(() =>
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
        }).ConfigureAwait(false);
    }

    /// <summary>
    ///     Apply a CustomizePlus profile to the local player in an additive way
    /// </summary>
    /// <param name="templateJson">Profile data, most commonly retrieved from <see cref="GetProfile"/> or via the "Copy Template" button in CustomizePlus UI</param>
    public async Task<bool> ApplyCustomizeAdditive(string? templateJson = null)
    {
        if (ApiAvailable is false)
            return false;

        if (_customizePlusPlugin is null)
            return false;
        
        if (Plugin.CharacterConfiguration is not { } character)
            return false;

        return await DalamudUtilities.RunOnFramework(() =>
        {
            CustomizePlusProfile profile;
            if (_customizePlusPlugin.ProfileManager.TryGetActiveProfileOnCharacter(character.Name) is not { } activeProfile)
            {
                // There is no active profile, so make one.
                if (_customizePlusPlugin.ProfileManager.CreateProfile() is not { } newProfile) return false;
                if (_customizePlusPlugin.ProfileManager.AddCharacter(newProfile) is false) return false;
                if (_customizePlusPlugin.ProfileManager.SetPriority(newProfile) is false) return false;
                if (_customizePlusPlugin.ProfileManager.SetEnabled(newProfile) is false) return false;
                profile = newProfile;
            }
            else
            {
                // We have the existing profile, now copy it
                if (_customizePlusPlugin.ProfileManager.Clone(activeProfile) is not { } cloned) return false;
                if (_customizePlusPlugin.ProfileManager.SetPriority(cloned) is false) return false;
                if (_customizePlusPlugin.ProfileManager.SetEnabled(cloned) is false) return false;
                profile = cloned;
            }

            // If template data was not provided, end early
            if (templateJson is null)
                return true;

            // Add the template data
            if (_customizePlusPlugin.TemplateManager.DeserializeTemplate(templateJson) is not { } template) return false;
            if (_customizePlusPlugin.ProfileManager.AddTemplate(profile, template) is false) return false;
            return true;
        }).ConfigureAwait(false);
    }
    
    /// <summary>
    ///     Deletes the temporary profile created by Aether Remote, if one exists
    /// </summary>
    public async Task<bool> DeleteTemporaryCustomizeAsync()
    {
        if (ApiAvailable is false)
            return false;

        try
        {
            return await DalamudUtilities.RunOnFramework(() => _customizePlusPlugin?.ProfileManager.DeleteTemporaryProfile() ?? false).ConfigureAwait(false);
        }
        catch (Exception e)
        {
            Plugin.Log.Error($"[CustomizePlusService.DeleteTemporaryCustomizeAsync] {e}");
            return false;
        }
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
            // Ignore until Dalamud introduces a proper async dispose
        }
    }
}