using System;
using System.Collections;
using System.Reflection;
using AetherRemoteClient.Dependencies.CustomizePlus.Reflection.Domain;

namespace AetherRemoteClient.Dependencies.CustomizePlus.Reflection.Managers;

/// <summary>
///     Domain encapsulation of a CustomizePlus ProfileManager class
/// </summary>
public class ProfileManager
{
    // Const
    private const string TemporaryProfileName = "AetherRemoteTemporaryProfile";
    private const int Priority = int.MaxValue - 32; // Give other plugins the opportunity to override if needed
    private const BindingFlags PublicInstance = BindingFlags.Public | BindingFlags.Instance;
    
    // Profile Manager
    private readonly object _profileManager;
    private readonly ActorManager _actorManager;
    private readonly MethodInfo _addCharacter;
    private readonly MethodInfo _addTemplate;
    private readonly MethodInfo _createProfile;
    private readonly MethodInfo _deleteProfile;
    private readonly MethodInfo _setEnabled;
    private readonly MethodInfo _setPriority;
    private readonly FieldInfo _profiles;
    private readonly PropertyInfo _profileName;
    
    /// <summary>
    ///     <inheritdoc cref="ProfileManager"/>
    /// </summary>
    private ProfileManager(
        object profileManager,
        ActorManager actorManager,
        MethodInfo addCharacter,
        MethodInfo addTemplate,
        MethodInfo createProfile,
        MethodInfo deleteProfile,
        MethodInfo setEnabled,
        MethodInfo setPriority,
        FieldInfo profiles,
        PropertyInfo profileName)
    {
        // Objects
        _profileManager = profileManager;
        _actorManager = actorManager;
        
        // Methods
        _addCharacter = addCharacter;
        _addTemplate = addTemplate;
        _createProfile = createProfile;
        _deleteProfile = deleteProfile;
        _setEnabled = setEnabled;
        _setPriority = setPriority;
        
        // Fields
        _profiles = profiles;
        
        // Properties
        _profileName = profileName;
    }

    /// <summary>
    ///     Creates a new CustomizePlus profile
    /// </summary>
    public CustomizePlusProfile? CreateProfile()
    {
        try
        {
            return _createProfile.Invoke(_profileManager, [TemporaryProfileName, true]) is { } profile
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

            _addCharacter.Invoke(_profileManager, [customizePlusProfile.Value, localCurrentPlayer]);
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
            _addTemplate.Invoke(_profileManager, [customizePlusProfile.Value, customizePlusTemplate.Value]);
            return true;
        }
        catch (Exception e)
        {
            Plugin.Log.Error($"[ProfileManager.AddTemplate] An error occurred, {e}");
            return false;
        }
    }

    /// <summary>
    ///     Sets the priority of the provided profile to <see cref="Priority"/>, which is 2,147,483,615
    /// </summary>
    /// <param name="customizePlusProfile">Reflected profile retrieved from <see cref="CreateProfile"/></param>
    public bool SetPriority(CustomizePlusProfile customizePlusProfile)
    {
        try
        {
            _setPriority.Invoke(_profileManager, [customizePlusProfile.Value, Priority]);
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
            _setEnabled.Invoke(_profileManager, [customizePlusProfile.Value, true, false]);
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
        
            _deleteProfile.Invoke(_profileManager, [profile.Value]);
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
            if (_profileName.GetValue(profile)?.ToString() is not { } name)
                continue;
            
            if (name == TemporaryProfileName)
                return new CustomizePlusProfile(profile);
        }

        return null;
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
            if (pluginType.Assembly.GetType("CustomizePlus.Profiles.Data.Profile") is not { } profileType)
                return null;
            
            // Get Manager Fields
            if (managerType.GetField("Profiles", PublicInstance) is not { } profilesField)
                return null;

            if (profileType.GetProperty("Name", PublicInstance) is not { } profileNameProperty)
                return null;
            
            // Get Manager Methods
            if (managerType.GetMethod("AddCharacter", PublicInstance) is not { } addCharacter) return null;
            if (managerType.GetMethod("AddTemplate", PublicInstance) is not { } addTemplate) return null;
            if (managerType.GetMethod("Create", PublicInstance) is not { } create) return null;
            if (managerType.GetMethod("Delete", PublicInstance) is not { } delete) return null;
            if (managerType.GetMethod("SetEnabled", PublicInstance, null, [profileType, typeof(bool), typeof(bool)], null) is not { } setEnabled) return null;
            if (managerType.GetMethod("SetPriority", PublicInstance, null, [profileType, typeof(int)], null) is not { } setPriority) return null;

            return new ProfileManager(profileManager, actorManager, addCharacter, addTemplate, create, delete, setEnabled, setPriority, profilesField, profileNameProperty);
        }
        catch (Exception e)
        {
            Plugin.Log.Error($"[ProfileManager.Create] An error occurred while reflecting, {e}");
            return null;
        }
    }
}