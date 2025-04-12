using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using AetherRemoteClient.Domain.Interfaces;
using AetherRemoteClient.Ipc.Domain;

namespace AetherRemoteClient.Ipc;

/*  ==================================
 *  To the developers of CustomizePlus
 *  ==================================
 *  Please make your IPCs return
 *  temporary profiles and templates
 *  where applicable. Thank you!
 *      - A shy dev.
 *  ==================================
 */

/// <summary>
///     Provides access to CustomizePlus
/// </summary>
public class CustomizePlusIpc : IExternalPlugin, IDisposable
{
    // Instantiated
    private ProfileManager _profileManager = null!;
    private TemplateManager _templateManager = null!;

    /// <summary>
    ///     Manages the last state this plugin was in on <see cref="TestIpcAvailability"/>
    /// </summary>
    private bool _lastPluginLoadedState;

    /// <summary>
    ///     Is CustomizePlus available for use?
    /// </summary>
    public bool ApiAvailable;

    /// <summary>
    ///     <inheritdoc cref="CustomizePlusIpc"/>
    /// </summary>
    public CustomizePlusIpc()
    {
        TestIpcAvailability();
    }

    /// <summary>
    ///     Tests for availability to CustomizePlus
    /// </summary>
    public void TestIpcAvailability()
    {
        if (Plugin.PluginInterface.InstalledPlugins.FirstOrDefault(p =>
                p.Name.Contains("Customize", StringComparison.OrdinalIgnoreCase)) is not { } plugin)
        {
            Plugin.Log.Verbose("CustomizePlus is not installed");
            ApiAvailable = false;
            return;
        }
        
        if (plugin.IsLoaded == _lastPluginLoadedState)
        {
            if (plugin.IsLoaded && ApiAvailable is false)
                Task.Run(TestIpcAvailabilityAsync);
        }
        else
        {
            if (plugin.IsLoaded)
                Task.Run(TestIpcAvailabilityAsync);
            else
                ApiAvailable = false;
        }
        
        _lastPluginLoadedState = plugin.IsLoaded;
    }

    /// <summary>
    ///     Since reflection is a lot of work, we will run this on a new IO thread
    /// </summary>
    private void TestIpcAvailabilityAsync()
    {
        ApiAvailable = false;
        try
        {
            var pluginInstance = TryGetCustomizePluginInstance();
            if (pluginInstance is null)
            {
                ApiAvailable = false;
                return;
            }

            _profileManager = new ProfileManager(pluginInstance);
            if (_profileManager.Initialize() is false)
            {
                ApiAvailable = false;
                return;
            }

            _templateManager = new TemplateManager(pluginInstance);
            if (_templateManager.Initialize() is false)
            {
                ApiAvailable = false;
                return;
            }

            ApiAvailable = true;
        }
        catch (Exception e)
        {
            Plugin.Log.Verbose($"TestIpcAvailability for CustomizePlus failed, {e.Message}");
        }
    }

    /// <summary>
    ///     Creates and applies a special CustomizePlus profile to the local player with provided template data
    /// </summary>
    public bool ApplyCustomize(string customizeData)
    {
        if (ApiAvailable is false)
        {
            Plugin.Log.Warning("[CustomizePlusIpc] Failed to apply customize profile because api is not available");
            return false;
        }

        try
        {
            if (_templateManager.CreateTemplate(customizeData) is not { } template)
            {
                Plugin.Log.Warning("[CustomizePlusIpc] Deserialize customize template");
                return false;
            }

            // Delete any current profiles
            DeleteCustomize();

            if (_profileManager.Create() is not { } profile)
            {
                Plugin.Log.Warning("[CustomizePlusIpc] Failed to create profile");
                return false;
            }

            _profileManager.AddCharacter(profile);
            _profileManager.AddTemplate(profile, template);
            _profileManager.SetPriority(profile);
            _profileManager.SetEnabled(profile);
            return true;
        }
        catch (Exception e)
        {
            Plugin.Log.Warning($"[CustomizePlusIpc] Failed to apply customize profile, {e.Message}");
        }

        return false;
    }
    
    /// <summary>
    ///     Creates and applies a special CustomizePlus profile to the local player with provided template data
    /// </summary>
    public bool ApplyCustomize(IList templates)
    {
        if (ApiAvailable is false)
        {
            Plugin.Log.Warning("[CustomizePlusIpc] Failed to apply customize profile because api is not available");
            return false;
        }

        try
        {
            // Delete any current profiles
            DeleteCustomize();
            
            if (_profileManager.Create() is not { } profile)
            {
                Plugin.Log.Warning("[CustomizePlusIpc] Failed to create profile");
                return false;
            }

            _profileManager.AddCharacter(profile);
            foreach (var template in templates)
                _profileManager.AddTemplate(profile, template);
            
            _profileManager.SetPriority(profile);
            _profileManager.SetEnabled(profile);
            return true;
        }
        catch (Exception e)
        {
            Plugin.Log.Warning($"[CustomizePlusIpc] Failed to apply customize profile, {e.Message}");
        }

        return false;
    }

    /// <summary>
    ///     Removes the CustomizePlus profile created by AR
    /// </summary>
    public bool DeleteCustomize()
    {
        if (ApiAvailable is false)
        {
            Plugin.Log.Warning("[CustomizePlusIpc] Failed to apply customize profile because api is not available");
            return false;
        }

        try
        {
            _profileManager.Delete();
            return true;
        }
        catch (Exception e)
        {
            Plugin.Log.Warning($"[CustomizePlusIpc] Failed to delete customize profile, {e.Message}");
        }

        return false;
    }

    /// <summary>
    ///     Gets all active templates on provided player name
    /// </summary>
    /// <returns></returns>
    public IList? GetActiveTemplatesOnCharacter(string characterName) =>
        _profileManager.GetActiveProfileOnCharacter(characterName);

    /// <summary>
    ///     Get the Customize plus plugin assembly from loaded into dalamud services
    /// </summary>
    private static object? TryGetCustomizePluginInstance()
    {
        try
        {
            var assemblies = Assembly.GetExecutingAssembly().GetReferencedAssemblies();
            Assembly? dalamud = null;
            foreach (var assembly in assemblies)
            {
                if (assembly.Name is null ||
                    assembly.Name.Contains("dalamud", StringComparison.OrdinalIgnoreCase) is false)
                    continue;

                dalamud = AppDomain.CurrentDomain.GetAssemblies().First(asm => asm.GetName().Name == assembly.Name);
                break;
            }

            if (dalamud is null)
            {
                Plugin.Log.Info("Could not find dalamud assembly");
                return null;
            }

            Type? pluginManagerType = null;
            Type? localPluginType = null;
            Type? serviceHelper = null;

            var count = 0;
            var types = dalamud.GetTypes();
            foreach (var type in types)
            {
                var fullName = type.FullName;
                if (fullName is null)
                    continue;

                if (fullName.Contains("PluginManager", StringComparison.OrdinalIgnoreCase))
                {
                    count++;
                    pluginManagerType = type;
                    continue;
                }

                if (fullName.Contains("LocalPlugin", StringComparison.OrdinalIgnoreCase))
                {
                    count++;
                    localPluginType = type;
                    continue;
                }

                if (fullName.Contains("ServiceHelpers", StringComparison.OrdinalIgnoreCase))
                {
                    count++;
                    serviceHelper = type;
                    continue;
                }

                if (count > 2)
                    break;
            }

            if (pluginManagerType is null || localPluginType is null || serviceHelper is null)
            {
                Plugin.Log.Info(
                    $"One of the three types is null: {pluginManagerType}, {localPluginType}, {serviceHelper}");
                return null;
            }

            var pluginInstanceField =
                localPluginType.GetField("instance", BindingFlags.NonPublic | BindingFlags.Instance);
            if (pluginInstanceField is null)
            {
                Plugin.Log.Info("Could not find instance field");
                return null;
            }

            var localPluginAssemblyName = localPluginType.GetProperty("AssemblyName");
            if (localPluginAssemblyName is null)
            {
                Plugin.Log.Info("Could not find AssemblyName");
                return null;
            }

            var methodGetAsService = serviceHelper.GetMethod("GetAsService");
            if (methodGetAsService is null)
            {
                Plugin.Log.Info("Could not find GetAsService method");
                return null;
            }

            if (methodGetAsService.Invoke(serviceHelper, [pluginManagerType]) is not Type pluginManagerServiceType)
            {
                Plugin.Log.Info("Could not find PluginManagerServiceType");
                return null;
            }

            var methodGetNullable = pluginManagerServiceType.GetMethod("GetNullable");
            if (methodGetNullable is null)
            {
                Plugin.Log.Info("Could not find GetNullable method");
                return null;
            }

            object value = 0;
            var pluginManagerObj = methodGetNullable.Invoke(pluginManagerServiceType, [value]);
            var property = pluginManagerType.GetProperty("InstalledPlugins");
            if (property is null)
            {
                Plugin.Log.Info("Could not find InstalledPlugins property");
                return null;
            }

            if (property.GetValue(pluginManagerObj) is not IList listLocalPlugins)
            {
                Plugin.Log.Info("Could not find InstalledPlugins list");
                return null;
            }

            foreach (var localPlugin in listLocalPlugins)
            {
                if (localPluginAssemblyName.GetValue(localPlugin) is not AssemblyName assemblyName)
                    continue;

                if (assemblyName.Name?.Contains("CustomizePlus", StringComparison.OrdinalIgnoreCase) ?? false)
                    return pluginInstanceField.GetValue(localPlugin);
            }
        }
        catch (Exception e)
        {
            Plugin.Log.Fatal(e.ToString());
        }

        Plugin.Log.Warning("Could not find CustomizePlus assembly");
        return null;
    }

    public void Dispose()
    {
        _profileManager.Delete();
        GC.SuppressFinalize(this);
    }
}