using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AetherRemoteClient.Domain.CustomizePlus;
using AetherRemoteClient.Domain.Interfaces;
using AetherRemoteClient.Utils;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Plugin.Ipc;
using Dalamud.Plugin.Ipc.Exceptions;
using CustomizePlusWrapper = AetherRemoteClient.Reflection.CustomizePlusWrapper;
using ProfileData = (
    System.Guid Id,
    string Name,
    string Path,
    System.Collections.Generic.List<(string Name, ushort World, byte Type, ushort SubType)>,
    int Priority,
    bool Enabled);

namespace AetherRemoteClient.Services;

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
    ///     Reflected wrapper for Customize Plus
    /// </summary>
    private CustomizePlusWrapper? _customizePlusWrapper;

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
        _customizePlusWrapper = null;
        ApiAvailable = false;
        
        try
        {
            // Invoke Api
            var (major, minor) = await DalamudUtilities.RunOnFramework(() => _getVersion.InvokeFunc()).ConfigureAwait(false);

            // Test for proper versioning
            if (major is not ExpectedMajor || minor < ExpectedMinor)
                return false;

            // Check to make sure the reflection process was successful
            if (await DalamudUtilities.RunOnFramework(CustomizePlusWrapper.Wrap).ConfigureAwait(false) is not { } customizePlusWrapper)
                return false;

            // Mark as ready
            _customizePlusWrapper = customizePlusWrapper;
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
    ///     Gets a customize plus profile by guid
    /// </summary>
    public async Task<string?> GetProfile(Guid guid)
    {
        if (ApiAvailable is false)
        {
            Plugin.Log.Warning("[CustomizePlusService.GetProfile] Api not available");
            return null;
        }
        
        try
        {
            var (error, json) = await DalamudUtilities.RunOnFramework(() => _getProfileById.InvokeFunc(guid)).ConfigureAwait(false);
            return error is 0 ? json : null;
        }
        catch (Exception e)
        {
            Plugin.Log.Warning($"[CustomizePlusService.GetProfile] An error occurred, {e}");
            return null;
        }
    }

    /// <summary>
    ///     Gets a list of all customize plus profiles
    /// </summary>
    public async Task<List<Profile>?> GetProfiles()
    {
        if (ApiAvailable is false)
        {
            Plugin.Log.Warning("[CustomizePlusService.GetProfilesPlain] Api not available");
            return null;
        }

        try
        {
            var result = await DalamudUtilities.RunOnFramework(() => _getProfileList.InvokeFunc()).ConfigureAwait(false);
            return result.Select(profile => new Profile(profile.Id, profile.Name, profile.Path)).ToList();
        }
        catch (Exception e)
        {
            Plugin.Log.Error($"[CustomizePlusService.GetProfilesPlain] {e}");
            return null;
        }
    }

    /// <summary>
    ///     Tries to get the template data for the active profile on a provided character
    /// </summary>
    /// <returns>The JSON template data, same as if called via GetProfileIpc</returns>
    public async Task<string?> TryGetActiveProfileOnCharacter(IPlayerCharacter character)
    {
        if (ApiAvailable is false || _customizePlusWrapper is null)
        {
            Plugin.Log.Warning("[CustomizePlusService.TryGetActiveProfileOnCharacter] Api not available");
            return null;
        }
        
        try
        {
            return await DalamudUtilities.RunOnFramework(() => _customizePlusWrapper.GetIpcProfile(character)).ConfigureAwait(false);
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
    /// <param name="json">Template data, most commonly retrieved from <see cref="GetProfile"/> or via the "Copy Template" button in CustomizePlus UI</param>
    public async Task<bool> ApplyCustomize(string? json = null)
    {
        if (ApiAvailable is false || _customizePlusWrapper is null)
        {
            Plugin.Log.Warning("[CustomizePlusService.ApplyCustomizeAsync] Api not available");
            return false;
        }
        
        try
        {
            return await DalamudUtilities.RunOnFramework(() => _customizePlusWrapper.DeleteTemporaryProfile() && _customizePlusWrapper.CreateTemporaryProfile(json)).ConfigureAwait(false);
        }
        catch (Exception e)
        {
            Plugin.Log.Error($"[CustomizePlusService.ApplyCustomizeAsync] {e}");
            return false;
        }
    }

    /// <summary>
    ///     Apply a CustomizePlus profile to the local player in an additive way
    /// </summary>
    /// <param name="json">Template data, most commonly retrieved from <see cref="GetProfile"/> or via the "Copy Template" button in CustomizePlus UI</param>
    public async Task<bool> ApplyMergeCustomize(string? json = null)
    {
        if (ApiAvailable is false || _customizePlusWrapper is null)
        {
            Plugin.Log.Warning("[CustomizePlusService.ApplyCustomizeAsync] Api not available");
            return false;
        }
        
        if (await DalamudUtilities.TryGetLocalPlayer().ConfigureAwait(false) is not { } character)
        {
            Plugin.Log.Warning("[CustomizePlusService.ApplyCustomizeAsync] Unable to locate local character");
            return false;
        }
        
        try
        {
            return await DalamudUtilities.RunOnFramework(() =>
            {
                var profile = _customizePlusWrapper.GetProfile(character);
                
                _customizePlusWrapper.DeleteTemporaryProfile();
                
                return profile is null
                    ? _customizePlusWrapper.CreateTemporaryProfile(json)
                    : _customizePlusWrapper.CloneTemporaryProfile(profile, json);
            }).ConfigureAwait(false);
        }
        catch (Exception e)
        {
            Plugin.Log.Error($"[CustomizePlusService.ApplyCustomizeAsync] {e}");
            return false;
        }
    }
    
    /// <summary>
    ///     Deletes the temporary profile created by Aether Remote, if one exists
    /// </summary>
    public async Task<bool> DeleteTemporaryCustomizeAsync()
    {
        if (ApiAvailable is false || _customizePlusWrapper is null)
        {
            Plugin.Log.Warning("[CustomizePlusService.DeleteTemporaryCustomizeAsync] Api not available");
            return false;
        }

        try
        {
            return await DalamudUtilities.RunOnFramework(() => _customizePlusWrapper.DeleteTemporaryProfile()).ConfigureAwait(false);
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