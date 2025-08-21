using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace AetherRemoteClient.Dependencies.Domain;

/// <summary>
///     Provides access to the Customize profile manager via reflection
/// </summary>
public class ProfileManager(object pluginInstance)
{
    private const string ProfileName = "AetherRemoteTemporaryProfile";

    private Type? _templateType;
    private MethodInfo? _addCharacter;
    private MethodInfo? _addTemplate;
    private MethodInfo? _createProfile;
    private MethodInfo? _deleteProfile;
    private MethodInfo? _setEnabled;
    private MethodInfo? _setPriority;
    private FieldInfo? _getCurrentCharacterField;
    private FieldInfo? _profilesField;
    private object? _instance;

    /// <summary>
    ///     Initialize the manager. This must be called before calling any function on this manager
    /// </summary>
    public bool Initialize()
    {
        try
        {
            var pluginType = pluginInstance.GetType();
            var servicesFieldInfo = pluginType.GetField("_services", BindingFlags.Instance | BindingFlags.NonPublic);
            var servicesInstance = servicesFieldInfo?.GetValue(pluginInstance);
            var getServiceMethodInfo = servicesInstance?.GetType().GetMethod("GetService");

            var assembly = pluginType.Assembly;
            var profileManagerType = assembly.GetType("CustomizePlus.Profiles.ProfileManager")!;
            var getProfileManagerMethod = getServiceMethodInfo?.MakeGenericMethod(profileManagerType);
            var profileManagerInstance = getProfileManagerMethod?.Invoke(servicesInstance, null);
            if (profileManagerInstance is null)
            {
                Plugin.Log.Fatal("Unable to get customize profile manager instance");
                return false;
            }

            var type = profileManagerInstance.GetType();
            _addCharacter = type.GetMethod("AddCharacter", BindingFlags.Public | BindingFlags.Instance);
            _addTemplate = type.GetMethod("AddTemplate", BindingFlags.Public | BindingFlags.Instance);
            _createProfile = type.GetMethod("Create", BindingFlags.Public | BindingFlags.Instance);
            _deleteProfile = type.GetMethod("Delete", BindingFlags.Public | BindingFlags.Instance);
            _setPriority = type.GetMethod("SetPriority", BindingFlags.Public | BindingFlags.Instance);
            _getCurrentCharacterField = type.GetField("_actorManager", BindingFlags.NonPublic | BindingFlags.Instance);
            _profilesField = type.GetField("Profiles", BindingFlags.Public | BindingFlags.Instance);
            _instance = profileManagerInstance;

            var profileType = assembly.GetType("CustomizePlus.Profiles.Data.Profile")!;
            _setEnabled = type.GetMethod("SetEnabled", BindingFlags.Public | BindingFlags.Instance, null,
                [profileType, typeof(bool), typeof(bool)], null);

            _templateType = assembly.GetType("CustomizePlus.Templates.Data.Template");

            /*
             * This is hacky, but disposing CustomizePlusIPC doesn't always remove the profile
             * So instead, when the plugin loads, clean up any aether remote profiles created before logout
             */
            Delete();

            return true;
        }
        catch (Exception e)
        {
            Plugin.Log.Fatal($"Could not initialize ProfileManager, {e}");
            return false;
        }
    }

    /// <summary>
    ///     Adds the local current character to a provided profile
    /// </summary>
    /// <param name="profile">Customize profile instance</param>
    public bool AddCharacter(object profile)
    {
        try
        {
            if (GetCurrentPlayer() is not { } actor)
            {
                Plugin.Log.Warning("Character actor is null");
                return false;
            }

            object[] param = [profile, actor];
            _addCharacter?.Invoke(_instance, param);
            return true;
        }
        catch (Exception e)
        {
            Plugin.Log.Warning($"Error adding character, {e}");
            return false;
        }
    }

    /// <summary>
    ///     Adds the provided template to a provided profile
    /// </summary>
    /// <param name="profile">Customize profile instance</param>
    /// <param name="template">Customize template instance</param>
    public bool AddTemplate(object profile, object template)
    {
        try
        {
            object[] param = [profile, template];
            _addTemplate?.Invoke(_instance, param);
            return true;
        }
        catch (Exception e)
        {
            Plugin.Log.Warning($"Error adding template, {e}");
            return false;
        }
    }

    /// <summary>
    ///     Creates a new Customize profile instance
    /// </summary>
    /// <returns></returns>
    public object? Create()
    {
        try
        {
            if (GetExistingProfile() is not null)
            {
                Plugin.Log.Warning("Profile already exists");
                return null;
            }

            object[] param = [ProfileName, true];
            return _createProfile?.Invoke(_instance, param);
        }
        catch (Exception e)
        {
            Plugin.Log.Warning($"Error creating profile, {e}");
            return null;
        }
    }

    /// <summary>
    ///     Deletes any profile created by Aether Remote
    /// </summary>
    public bool Delete()
    {
        try
        {
            if (GetExistingProfile() is not { } profile)
            {
                Plugin.Log.Verbose("[ProfileManager] Profile doesn't exist, skipping delete");
                return true; // Exit gracefully
            }

            object[] param = [profile];
            _deleteProfile?.Invoke(_instance, param);
            return true;
        }
        catch (Exception e)
        {
            Plugin.Log.Warning($"Error deleting profile, {e}");
            return false;
        }
    }

    /// <summary>
    ///     Sets the enabled status for a provided profile
    /// </summary>
    public bool SetEnabled(object profile)
    {
        try
        {
            object[] param = [profile, true, false];
            _setEnabled?.Invoke(_instance, param);
            return true;
        }
        catch (Exception e)
        {
            Plugin.Log.Warning($"Error setting enabled, {e.Message}");
            return false;
        }
    }

    /// <summary>
    ///     Sets a priority for a provided profile
    /// </summary>
    /// <param name="profile"></param>
    public bool SetPriority(object profile)
    {
        try
        {
            object[] param = [profile, int.MaxValue];
            _setPriority?.Invoke(_instance, param);
            return true;
        }
        catch (Exception e)
        {
            Plugin.Log.Warning($"Error setting priority, {e}");
            return false;
        }
    }

    /// <summary>
    ///     Gets the existing Aether Remote profile
    /// </summary>
    /// <returns>Profile instance, otherwise null</returns>
    private object? GetExistingProfile()
    {
        try
        {
            var profiles = _profilesField?.GetValue(_instance);
            if (profiles is not IEnumerable profilesList)
            {
                Plugin.Log.Warning("ProfilesList is not an enumerable, aborting");
                return null;
            }

            foreach (var profile in profilesList)
            {
                var profileType = profile.GetType();
                var nameField = profileType.GetProperty("Name", BindingFlags.Public | BindingFlags.Instance);
                var nameObject = nameField?.GetValue(profile);
                if (nameObject?.ToString() is not { } name ||
                    name.Contains(ProfileName, StringComparison.OrdinalIgnoreCase) is false)
                    continue;

                return profile;
            }
        }
        catch (Exception e)
        {
            Plugin.Log.Warning($"Error retrieving existing profile, {e}");
        }
        
        return null;
    }

    /// <summary>
    ///     Searches for the active profile, and retrieves the active template
    /// </summary>
    /// <returns>Profile, or null if no profile is found</returns>
    public IList? GetActiveProfileOnCharacter(string name)
    {
        try
        {
            var profiles = _profilesField?.GetValue(_instance);
            if (profiles is not IEnumerable profilesList)
                return null;

            object? finalProfile = null;
            var highestPriorityFound = -1;
            foreach (var profile in profilesList)
            {
                var profileType = profile.GetType();
                var enabledField = profileType.GetProperty("Enabled", BindingFlags.Public | BindingFlags.Instance);
                var enabledBool = enabledField?.GetValue(profile);
                if (enabledBool is null or false)
                    continue;

                var charactersField =
                    profileType.GetProperty("Characters", BindingFlags.Public | BindingFlags.Instance);
                var charactersObject = charactersField?.GetValue(profile);
                if (charactersObject is not IList characters)
                {
                    Plugin.Log.Warning("Characters list is not an enumerable, aborting");
                    continue;
                }

                var foundCurrentLocalCharacter = false;
                foreach (var character in characters)
                {
                    if (character is null ||
                        character.ToString()?.Contains(name, StringComparison.OrdinalIgnoreCase) is false)
                        continue;

                    foundCurrentLocalCharacter = true;
                    break;
                }

                if (foundCurrentLocalCharacter is false)
                    continue;

                var priorityField = profileType.GetProperty("Priority", BindingFlags.Public | BindingFlags.Instance);
                var priorityObject = priorityField?.GetValue(profile);
                if (priorityObject is not int priority)
                {
                    Plugin.Log.Warning("Characters list is not an enumerable, aborting");
                    continue;
                }

                if (priority <= highestPriorityFound)
                    continue;

                highestPriorityFound = priority;
                finalProfile = profile;
            }

            // No active or valid profiles for this character
            if (finalProfile is null)
            {
                if (_templateType is null)
                    return null;

                try
                {
                    var emptyTemplate = Activator.CreateInstance(_templateType);
                    var list = (IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(_templateType))!;
                    list.Add(emptyTemplate);
                    return list;
                }
                catch (Exception e)
                {
                    Plugin.Log.Warning($"[ProfileManager] Exception, {e}");
                    return null;
                }
            }

            var finalProfileType = finalProfile.GetType();
            var templatesField = finalProfileType.GetProperty("Templates", BindingFlags.Public | BindingFlags.Instance);
            var templatesObject = templatesField?.GetValue(finalProfile);
            return templatesObject as IList;
        }
        catch (Exception e)
        {
            Plugin.Log.Warning($"Error getting active profile, {e}");
            return null;
        }
    }

    /// <summary>
    ///     Gets the current player as a Penumbra.Data.Actor instance
    /// </summary>
    /// <returns></returns>
    private object? GetCurrentPlayer()
    {
        try
        {
            var actorManager = _getCurrentCharacterField?.GetValue(_instance);
            var getCurrentPlayerMethod = actorManager?.GetType().GetMethod("GetCurrentPlayer");
            return getCurrentPlayerMethod?.Invoke(actorManager, null);
        }
        catch (Exception e)
        {
            Plugin.Log.Warning($"[ProfileManager] Error getting current player, {e}");
            return null;
        }
    }
}