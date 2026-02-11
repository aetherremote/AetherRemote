using System;
using System.Collections;
using System.Reflection;
using AetherRemoteClient.Dependencies.CustomizePlus.Reflection.Domain;
using AetherRemoteClient.Dependencies.CustomizePlus.Reflection.Domain.Containers;
using Newtonsoft.Json;

namespace AetherRemoteClient.Dependencies.CustomizePlus.Reflection.Managers;

/// <summary>
///     Domain encapsulation of a CustomizePlus ProfileManager class
/// </summary>
public class ProfileManager
{
    // Const
    private const string TemporaryProfileName = "AetherRemoteTemporaryProfile";
    private const int TemporaryProfilePriority = int.MaxValue - 32; // Give other plugins the opportunity to override if needed
    private const BindingFlags PublicInstance = BindingFlags.Public | BindingFlags.Instance;
    private const BindingFlags PublicStatic = BindingFlags.Public | BindingFlags.Static;
    private static readonly JsonSerializerSettings SerializerSettings = new() { DefaultValueHandling = DefaultValueHandling.Ignore };
    
    // Profile Manager
    private readonly object _profileManager;
    private readonly ActorManager _actorManager;
    private readonly FieldInfo _profiles;
    private readonly IpcCharacterProfileContainer _ipcCharacterProfileContainer;
    private readonly ProfileContainer _profileContainer;
    private readonly ProfileManagerContainer _profileManagerContainer;
    
    /// <summary>
    ///     <inheritdoc cref="ProfileManager"/>
    /// </summary>
    private ProfileManager(object profileManager, ActorManager actorManager, FieldInfo profiles, IpcCharacterProfileContainer ipcCharacterProfileContainer, ProfileContainer profileContainer, ProfileManagerContainer profileManagerContainer)
    {
        // Objects
        _profileManager = profileManager;
        _actorManager = actorManager;
        
        // Fields
        _profiles = profiles;
        
        // Containers
        _ipcCharacterProfileContainer = ipcCharacterProfileContainer;
        _profileContainer = profileContainer;
        _profileManagerContainer = profileManagerContainer;
    }

    /// <summary>
    ///     Creates a new CustomizePlus profile
    /// </summary>
    public CustomizePlusProfile? CreateProfile()
    {
        try
        {
            return _profileManagerContainer.Create.Invoke(_profileManager, [TemporaryProfileName, true]) is { } profile
                ? new CustomizePlusProfile(profile)
                : null;
        }
        catch (Exception e)
        {
            Plugin.Log.Warning($"[ProfileManager.CreateProfile] An error occurred, {e}");
            return null;
        }
    }
    
    /// <summary>
    ///     Adds the local character to the provided profile
    /// </summary>
    /// <param name="customizePlusProfile">Reflected profile retrieved from <see cref="CreateProfile"/></param>
    public bool AddCharacter(CustomizePlusProfile customizePlusProfile)
    {
        try
        {
            if (_actorManager.GetCurrentPlayer() is not { } localCurrentPlayer)
                return false;

            _profileManagerContainer.AddCharacter.Invoke(_profileManager, [customizePlusProfile.Value, localCurrentPlayer]);
            return true;
        }
        catch (Exception e)
        {
            Plugin.Log.Error($"[ProfileManager.AddCharacter] An error occurred, {e}");
            return false;
        }
    }
    
    /// <summary>
    ///     Adds the local character to the provided profile
    /// </summary>
    /// <param name="customizePlusProfile">Reflected profile retrieved from <see cref="CreateProfile"/></param>
    /// <param name="customizePlusTemplate">Reflected template retrieved from <see cref="TemplateManager.DeserializeTemplate"/></param>
    public bool AddTemplate(CustomizePlusProfile customizePlusProfile, CustomizePlusTemplate customizePlusTemplate)
    {
        try
        {
            _profileManagerContainer.AddTemplate.Invoke(_profileManager, [customizePlusProfile.Value, customizePlusTemplate.Value]);
            return true;
        }
        catch (Exception e)
        {
            Plugin.Log.Error($"[ProfileManager.AddTemplate] An error occurred, {e}");
            return false;
        }
    }

    /// <summary>
    ///     Sets the priority of the provided profile to <see cref="TemporaryProfilePriority"/>, which is 2,147,483,615
    /// </summary>
    /// <param name="customizePlusProfile">Reflected profile retrieved from <see cref="CreateProfile"/></param>
    public bool SetPriority(CustomizePlusProfile customizePlusProfile)
    {
        try
        {
            _profileManagerContainer.SetPriority.Invoke(_profileManager, [customizePlusProfile.Value, TemporaryProfilePriority]);
            return true;
        }
        catch (Exception e)
        {
            Plugin.Log.Error($"[ProfileManager.SetPriority] An error occurred, {e}");
            return false;
        }
    }

    /// <summary>
    ///     Sets the provided profile to enabled
    /// </summary>
    /// <param name="customizePlusProfile">Reflected profile retrieved from <see cref="CreateProfile"/></param>
    public bool SetEnabled(CustomizePlusProfile customizePlusProfile)
    {
        try
        {
            _profileManagerContainer.SetEnabled.Invoke(_profileManager, [customizePlusProfile.Value, true, false]);
            return true;
        }
        catch (Exception e)
        {
            Plugin.Log.Error($"[ProfileManager.SetEnabled] An error occurred, {e}");
            return false;
        }
    }

    /// <summary>
    ///     Deletes the temporary profile created by aether remote, if one exists
    /// </summary>
    public bool DeleteTemporaryProfile()
    {
        try
        {
            if (TryGetTemporaryProfile() is not { } profile)
                return true; // Exit gracefully
        
            _profileManagerContainer.Delete.Invoke(_profileManager, [profile.Value]);
            return true;
        }
        catch (Exception e)
        {
            Plugin.Log.Error($"[ProfileManager.DeleteTemporaryProfile] An error occurred, {e}");
            return false;
        }
    }
    
    /// <summary>
    ///     Attempts to get the temporary profile created by aether remote, if one exists
    /// </summary>
    private CustomizePlusProfile? TryGetTemporaryProfile()
    {
        if (_profiles.GetValue(_profileManager) is not IEnumerable profiles)
            return null;

        foreach (var profile in profiles)
        {
            if (_profileContainer.Name.GetValue(profile)?.ToString() is not { } name)
                continue;
            
            if (name == TemporaryProfileName)
                return new CustomizePlusProfile(profile);
        }

        return null;
    }

    /// <summary>
    ///     Attempts to get the highest priority active profile for a character
    /// </summary>
    public string? TryGetActiveIpcProfileOnCharacter(string characterNameToSearchFor)
    {
        // If we couldn't find a customize profile on the character, that just means they don't have one set
        if (TryGetActiveProfileOnCharacter(characterNameToSearchFor) is not { } profile)
            return string.Empty;

        if (_ipcCharacterProfileContainer.FromFullProfile.Invoke(null, [profile]) is not { } ipcCharacterProfile)
            return null;
        
        try
        {
            return JsonConvert.SerializeObject(ipcCharacterProfile, SerializerSettings);
        }
        catch (Exception e)
        {
            Plugin.Log.Warning($"[ProfileManager.TryGetActiveIpcProfileOnCharacter] An error occurred, {e}");
            return null;
        }
    }

    /// <summary>
    ///     Clones an existing profile
    /// </summary>
    public CustomizePlusProfile? Clone(CustomizePlusProfile profile)
    {
        try
        {
            return _profileManagerContainer.Duplicate.Invoke(_profileManager, [profile.Value, TemporaryProfileName, false]) is { } cloned
                ? new CustomizePlusProfile(cloned)
                : null;
        }
        catch (Exception e)
        {
            Plugin.Log.Warning($"[ProfileManager.Clone] An error occurred, {e}");
            return null;
        }
    }
    
    /// <summary>
    ///     Attempts to get the highest priority profile for a character
    /// </summary>
    public CustomizePlusProfile? TryGetActiveProfileOnCharacter(string characterNameToSearchFor)
    {
        if (_profiles.GetValue(_profileManager) is not IEnumerable profiles)
            return null;

        var highestPriority = -1;
        object? highestPriorityProfile = null;
        foreach (var profile in profiles)
        {
            // Check Enabled
            if (_profileContainer.Enabled.GetValue(profile) is false or null)
                continue;
            
            // Check Character
            if (_profileContainer.Characters.GetValue(profile) is not IEnumerable characters)
                continue;

            // Check to see if the character is one we care about
            var containsCharacter = false;
            foreach (var character in characters)
            {
                // Returns a string that looks like "First Last (World)"
                if (character?.ToString()?.Contains(characterNameToSearchFor) is null or false)
                    continue;

                containsCharacter = true;
                break;
            }

            if (containsCharacter is false)
                continue;

            // Check Priority
            if (_profileContainer.Priority.GetValue(profile) is not int priority)
                continue;

            if (priority <= highestPriority)
                continue;
            
            // Set the current highest profile
            highestPriority = priority;
            highestPriorityProfile = profile;
        }

        return highestPriorityProfile is null 
            ? null 
            : new CustomizePlusProfile(highestPriorityProfile);
    }

    /// <summary>
    ///     Creates a new instance of the ProfileManager
    /// </summary>
    /// <remarks>
    ///     Ideally this is called only a single time to not incur multiple reflection calls
    /// </remarks>
    public static ProfileManager? Create(object pluginInstance)
    {
        try
        {
            // Get Plugin Type
            var pluginType = pluginInstance.GetType();
            
            // Get Manager Instance
            if (pluginType.GetField("_services", BindingFlags.NonPublic | BindingFlags.Instance)?.GetValue(pluginInstance) is not { } servicesField)
                return null;

            if (servicesField.GetType().GetMethod("GetService") is not { } getServiceMethod)
                return null;

            if (pluginType.Assembly.GetType("CustomizePlus.Profiles.ProfileManager") is not { } profileManagerType)
                return null;

            if (getServiceMethod.MakeGenericMethod(profileManagerType).Invoke(servicesField, null) is not { } profileManager)
                return null;
            
            // Get Actor Manager
            if (ActorManager.Create(profileManager) is not { } actorManager)
                return null;
            
            // Get Manager Types
            var managerType = profileManager.GetType();
            
            // Get Manager Fields
            if (managerType.GetField("Profiles", PublicInstance) is not { } profilesField) return null;
            
            // Get Profile Types
            if (pluginType.Assembly.GetType("CustomizePlus.Profiles.Data.Profile") is not { } profileType) return null;
            if (profileType.GetProperty("Name", PublicInstance) is not { } name) return null;
            if (profileType.GetProperty("Enabled", PublicInstance) is not { } enabled) return null;
            if (profileType.GetProperty("Priority", PublicInstance) is not { } priority) return null;
            if (profileType.GetProperty("Characters", PublicInstance) is not { } characters) return null;
            var profileContainer = new ProfileContainer(name, enabled, priority, characters);
            
            // Get IPCCharacterProfile Types
            if (pluginType.Assembly.GetType("CustomizePlus.Api.Data.IPCCharacterProfile") is not { } ipcCharacterProfileType) return null;
            if (ipcCharacterProfileType.GetMethod("FromFullProfile", PublicStatic) is not { } fromFullProfile) return null;
            var ipcCharacterProfileContainer = new IpcCharacterProfileContainer(fromFullProfile);
            
            // Get Manager Methods
            if (managerType.GetMethod("AddCharacter", PublicInstance) is not { } addCharacter) return null;
            if (managerType.GetMethod("AddTemplate", PublicInstance) is not { } addTemplate) return null;
            if (managerType.GetMethod("Create", PublicInstance) is not { } create) return null;
            if (managerType.GetMethod("Clone", PublicInstance) is not { } clone) return null;
            if (managerType.GetMethod("Delete", PublicInstance) is not { } delete) return null;
            if (managerType.GetMethod("SetEnabled", PublicInstance, null, [profileType, typeof(bool), typeof(bool)], null) is not { } setEnabled) return null;
            if (managerType.GetMethod("SetPriority", PublicInstance, null, [profileType, typeof(int)], null) is not { } setPriority) return null;
            var managerContainer = new ProfileManagerContainer(addCharacter, addTemplate, create, clone, delete, setEnabled, setPriority);
            
            return new ProfileManager(profileManager, actorManager, profilesField, ipcCharacterProfileContainer, profileContainer, managerContainer);
        }
        catch (Exception e)
        {
            Plugin.Log.Error($"[ProfileManager.Create] An error occurred while reflecting, {e}");
            return null;
        }
    }
}