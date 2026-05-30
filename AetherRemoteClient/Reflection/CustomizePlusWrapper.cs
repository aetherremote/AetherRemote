using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Newtonsoft.Json;

namespace AetherRemoteClient.Reflection;

/// <summary>
///     Reflected wrapper for customize plus
/// </summary>
public partial class CustomizePlusWrapper
{
    // Const
    private const int TemporaryProfilePriority = int.MaxValue - 32;
    private const string TemporaryProfileName = "AetherRemoteTemporaryProfile";
    private const BindingFlags PublicStatic = BindingFlags.Static | BindingFlags.Public;
    private const BindingFlags PublicInstance = BindingFlags.Instance | BindingFlags.Public;
    private const BindingFlags PrivateInstance = BindingFlags.Instance | BindingFlags.NonPublic;
    private static readonly JsonSerializerSettings SerializerSettings = new() { DefaultValueHandling = DefaultValueHandling.Ignore };
    
    // Wrappers (for organization mainly)
    private readonly CustomizePlusActorIdentifierWrapper _actorIdentifier;
    private readonly CustomizePlusActorManagerWrapper _actorManager;
    private readonly CustomizePlusIpcCharacterProfileWrapper _ipcCharacterProfile;
    private readonly CustomizePlusProfileManagerWrapper _profileManager;
    private readonly CustomizePlusProfileWrapper _profile;
    private readonly CustomizePlusTemplateWrapper _template;
    
    /// <summary>
    ///     <inheritdoc cref="CustomizePlusWrapper"/>
    /// </summary>
    private CustomizePlusWrapper(
        CustomizePlusActorIdentifierWrapper actorIdentifier,
        CustomizePlusActorManagerWrapper actorManager,
        CustomizePlusIpcCharacterProfileWrapper ipcCharacterProfile,
        CustomizePlusProfileManagerWrapper profileManager,
        CustomizePlusProfileWrapper profile,
        CustomizePlusTemplateWrapper template)
    {
        _actorIdentifier = actorIdentifier;
        _actorManager = actorManager;
        _ipcCharacterProfile = ipcCharacterProfile;
        _profileManager = profileManager;
        _profile = profile;
        _template = template;
    }
    
    /// <summary>
    ///     Creates a new instance of the <see cref="CustomizePlusWrapper"/> by reflecting the plugin assembly and required components 
    /// </summary>
    public static CustomizePlusWrapper? Wrap()
    {
        if (TryGetCustomizePlusInstance() is not { } pluginInstance) return null;
        
        var pluginType = pluginInstance.GetType();
        var pluginAssembly = pluginType.Assembly;
        
        // Get various types
        if (pluginAssembly.GetType("CustomizePlus.Profiles.Data.Profile") is not { } profileType) return null;
        if (pluginAssembly.GetType("CustomizePlus.Templates.Data.Template") is not { } templateType) return null;
        if (pluginAssembly.GetType("CustomizePlus.Profiles.ProfileManager") is not { } profileManagerType) return null;
        if (pluginAssembly.GetType("CustomizePlus.Api.Data.IPCCharacterProfile") is not { } ipcCharacterProfileType) return null;
        
        // Get profile manager instance
        if (pluginType.GetField("_services", PrivateInstance)?.GetValue(pluginInstance) is not {} services) return null;
        if (services.GetType().GetMethod("GetService") is not { } getServiceMethod) return null;
        if (getServiceMethod.MakeGenericMethod(profileManagerType).Invoke(services, null) is not { } profileManagerInstance) return null;
        
        // These wrappers exist primarily for organization and separation of responsibilities.
        
        if (CustomizePlusActorIdentifierWrapper.Wrap(profileManagerType, profileManagerInstance) is not { } actorIdentifier) return null;
        if (CustomizePlusActorManagerWrapper.Wrap(profileManagerType, profileManagerInstance) is not { } actorManager) return null;
        if (CustomizePlusProfileManagerWrapper.Wrap(profileType, profileManagerType, profileManagerInstance) is not { } profileManager) return null;
        if (CustomizePlusIpcCharacterProfileWrapper.Wrap(ipcCharacterProfileType) is not { } ipcCharacterProfile) return null;
        if (CustomizePlusProfileWrapper.Wrap(profileType) is not { } profile) return null;
        if (CustomizePlusTemplateWrapper.Wrap(templateType) is not { } template) return null;
        
        // At this point return
        return new CustomizePlusWrapper(actorIdentifier, actorManager, ipcCharacterProfile, profileManager, profile, template);
    }

    /// <summary>
    ///     Creates a temporary profile that is enabled with the current character added to it 
    /// </summary>
    /// <param name="json">JSON template data to add to the profile</param>
    public bool CreateTemporaryProfile(string? json = null)
    {
        try
        {
            var profile = _profileManager.CreateProfile(TemporaryProfileName);
            if (_actorManager.GetCurrentPlayer() is not { } actorIdentifier) return false;
            if (_profileManager.AddCharacterToProfile(profile, actorIdentifier) is false) return false;
            _profileManager.SetProfilePriority(profile, TemporaryProfilePriority);
            _profileManager.SetProfileEnabled(profile, true);

            if (json is null)
                return true;

            if (_template.Deserialize(json) is not { } template) return false;
            _profileManager.AddTemplateToProfile(profile, template);

            return true;
        }
        catch (Exception e)
        {
            Plugin.Log.Error($"[CustomizePlusWrapper.CreateTemporaryProfile] {e}");
            return false;
        }
    }

    /// <summary>
    ///     Creates a temporary profile that is enabled with the current character added to it 
    /// </summary>
    /// <param name="profile">The Profile </param>
    /// <param name="json">JSON template data to add to the profile</param>
    public bool CloneTemporaryProfile(object profile, string? json = null)
    {
        try
        {
            var cloned = _profileManager.CloneProfile(profile, TemporaryProfileName);
            _profileManager.SetProfilePriority(cloned, TemporaryProfilePriority);
            _profileManager.SetProfileEnabled(cloned, true);

            if (json is null)
                return true;

            if (_template.Deserialize(json) is not { } template) return false;
            _profileManager.AddTemplateToProfile(cloned, template);

            return true;
        }
        catch (Exception e)
        {
            Plugin.Log.Error($"[CustomizePlusWrapper.CreateTemporaryProfile] {e}");
            return false;
        }
    }
    
    /// <summary>
    ///     Gracefully deletes the temporary profile if it exists
    /// </summary>
    public bool DeleteTemporaryProfile()
    {
        try
        {
            foreach (var profile in _profileManager.GetProfiles())
                if (_profile.GetName(profile) is TemporaryProfileName)
                    _profileManager.DeleteProfile(profile);

            return true;
        }
        catch (Exception e)
        {
            Plugin.Log.Error($"[CustomizePlusWrapper.DeleteTemporaryProfile] {e}");
            return false;
        }
    }

    /// <summary>
    ///     Returns the target character's profile
    /// </summary>
    public object? GetProfile(IPlayerCharacter targetCharacter)
    {
        var name = targetCharacter.Name.ToString();
        var world = (ushort)targetCharacter.HomeWorld.RowId;
        
        try
        {
            // Get the profile among the ones that are enabled, contain the specified character, and pick the one with the highest priority
            return  _profileManager.GetProfiles()
                .Where(_profile.GetEnabled)
                .Where(ContainsCharacter)
                .MaxBy(_profile.GetPriority);
        }
        catch (Exception e)
        {
            Plugin.Log.Error($"[CustomizePlusWrapper.GetProfile] {e}");
            return null;
        }

        // Checks to see if the profile contains the provided character
        bool ContainsCharacter(object profile)
        {
            foreach (var character in _profile.GetCharacters(profile))
                if (name == _actorIdentifier.GetCharacterName(character) && world == _actorIdentifier.GetCharacterWorld(character))
                    return true;
            
            return false;
        }
    }

    /// <summary>
    ///     Returns the target character's profile as an Ipc profile (which is essentially just template data as JSON)
    /// </summary>
    public string? GetIpcProfile(IPlayerCharacter targetCharacter)
    {
        try
        {
            if (GetProfile(targetCharacter) is not { } profile)
                return "{}"; // Return {} here since this will be used as an empty JSON object
            
            return JsonConvert.SerializeObject(_ipcCharacterProfile.FromFullProfile(profile), SerializerSettings);
        }
        catch (Exception e)
        {
            Plugin.Log.Error($"[CustomizePlusWrapper.GetIpcProfile] {e}");
            return null;
        }
    }
    
    /// <summary>
    ///     Attempts to actually reference the customize plus assembly for future reflected calls
    /// </summary>
    private static object? TryGetCustomizePlusInstance()
    {
        try
        {
            // Start constructing Dalamud reflection references
            if (AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(assembly => string.Equals(assembly.GetName().Name, "Dalamud", StringComparison.OrdinalIgnoreCase)) is not { } dalamudAssembly)
            {
                Plugin.Log.Error("[CustomizePlusWrapper.TryGetCustomizePlusInstance] Could not find Dalamud assembly?? This should never happen??");
                return null;
            }
            
            // Update if Dalamud ever changes the location of these types
            var pluginManagerType = dalamudAssembly.GetType("Dalamud.Plugin.Internal.PluginManager");
            var localPluginType = dalamudAssembly.GetType("Dalamud.Plugin.Internal.Types.LocalPlugin");
            
            // Service helper isn't a part of the direct assembly? I can't find it by scoping to it as Dalamud.ServiceHelper
            var serviceHelperType = dalamudAssembly.GetTypes().FirstOrDefault(type => type.FullName?.Contains("ServiceHelper", StringComparison.OrdinalIgnoreCase) is true);
            
            // https://github.com/goatcorp/Dalamud/tree/master/Dalamud/Plugin
            if (pluginManagerType is null || localPluginType is null || serviceHelperType is null)
            {
                Plugin.Log.Error($"{pluginManagerType is null} - {localPluginType is null} - {serviceHelperType is null}");
                Plugin.Log.Error("[CustomizePlusWrapper.TryGetCustomizePlusInstance] Required Dalamud types missing. If you see this please report to a developer immediately.");
                return null;
            }

            if (serviceHelperType.GetMethod("GetAsService", BindingFlags.Public | BindingFlags.Static) is not { } getAsService)
            {
                Plugin.Log.Error("[CustomizePlusWrapper.TryGetCustomizePlusInstance] GetAsService method not found. If you see this please report to a developer immediately.");
                return null;
            }
            
            if (getAsService.Invoke(null, [pluginManagerType]) is not Type serviceType)
            {
                Plugin.Log.Error("[CustomizePlusWrapper.TryGetCustomizePlusInstance] Failed to resolve PluginManager service type. If you see this please report to a developer immediately.");
                return null;
            }

            if (serviceType.GetMethod("GetNullable", BindingFlags.Public | BindingFlags.Static) is not { } getNullable)
            {
                Plugin.Log.Error("[CustomizePlusWrapper.TryGetCustomizePlusInstance] GetNullable method not found. If you see this please report to a developer immediately.");
                return null;
            }
            
            if (getNullable.Invoke(null, [0]) is not { } pluginManager)
            {
                Plugin.Log.Error("[CustomizePlusWrapper.TryGetCustomizePlusInstance] Failed to get PluginManager. If you see this please report to a developer immediately.");
                return null;
            }
            
            // Dalamud is done, now we can start searching for Customize+
            if (pluginManagerType.GetProperty("InstalledPlugins")?.GetValue(pluginManager) is not IList installedPlugins)
            {
                Plugin.Log.Error("[CustomizePlusWrapper.TryGetCustomizePlusInstance] Failed to get InstalledPlugins. If you see this please report to a developer immediately.");
                return null;
            }
            
            var assemblyProperty = localPluginType.GetProperty("Assembly");
            var instanceField = localPluginType.GetField("instance", BindingFlags.NonPublic | BindingFlags.Instance);
            if (assemblyProperty is null || instanceField is null)
            {
                Plugin.Log.Error("[CustomizePlusWrapper.TryGetCustomizePlusInstance] Required LocalPlugin members missing. If you see this please report to a developer immediately.");
                return null;
            }

            foreach (var plugin in installedPlugins)
            {
                if (assemblyProperty.GetValue(plugin) is not Assembly assembly)
                    continue;

                if (string.Equals(assembly.GetName().Name, "CustomizePlus", StringComparison.OrdinalIgnoreCase) is false)
                    continue;

                if (instanceField.GetValue(plugin) is { } instance)
                    return instance;
            }
            
            Plugin.Log.Verbose("[CustomizePlusWrapper.TryGetCustomizePlusInstance] CustomizePlus not installed.");
            return null;
        }
        catch (Exception e)
        {
            Plugin.Log.Error($"[CustomizePlusWrapper.TryGetCustomizePlusInstance] {e}");
            return null;
        }
    }
}