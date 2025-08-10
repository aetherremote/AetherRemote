using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using AetherRemoteClient.Domain.Interfaces;
using AetherRemoteClient.Ipc.Domain;
using Dalamud.Plugin;

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
        Plugin.PluginInterface.ActivePluginsChanged += PluginInterfaceOnActivePluginsChanged;
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
    public async Task<bool> ApplyCustomize(string customizeData)
    {
        if (ApiAvailable is false)
        {
            Plugin.Log.Warning("[CustomizePlusIpc] Failed to apply customize profile because api is not available");
            return false;
        }

        if (await Plugin.RunOnFramework(() => _templateManager.CreateTemplate(customizeData)).ConfigureAwait(false) is { } template)
            return await ApplyCustomize(new ArrayList { template }).ConfigureAwait(false);
        
        Plugin.Log.Warning("[CustomizePlusIpc] Failed to deserialize CustomizePlus template");
        return false;
    }

    /// <summary>
    ///     Deserialize templates from JSON string and apply them
    /// </summary>
    /// <param name="serializedData"></param>
    /// <returns></returns>
    public async Task<bool> DeserializeAndApplyCustomize(string serializedData)
    {
        if (_templateManager.DeserializeTemplates(serializedData) is { } templates)
            return await ApplyCustomize(templates).ConfigureAwait(false);
        
        Plugin.Log.Warning("[CustomizePlusIpc] Failed to deserialize saved template data");
        return false;
    }
    
    /// <summary>
    ///     Creates and applies a special CustomizePlus profile to the local player with provided template data
    /// </summary>
    public async Task<bool> ApplyCustomize(IList templates)
    {
        if (ApiAvailable)
            return await Plugin.RunOnFramework(() =>
            {
                if (_profileManager.Create() is not { } profile)
                {
                    Plugin.Log.Warning("[CustomizePlusIpc] Failed to create profile");
                    return false;
                }
                
                if (_profileManager.AddCharacter(profile) is false)
                {
                    Plugin.Log.Warning("[CustomizePlusIpc] Failed to add character");
                    return false;
                }

                // Check Type
                if (templates is List<object>)
                {
                    var converted = _templateManager.ConvertToTemplateType(templates);
                    if (converted is null)
                    {
                        Plugin.Log.Warning("[CustomizePlusIpc] Failed to convert templates");
                        return false;
                    }

                    templates = converted;
                }
                
                foreach (var template in templates)
                {
                    if (_profileManager.AddTemplate(profile, template))
                        continue;
                    
                    Plugin.Log.Warning("[CustomizePlusIpc] Failed to add a template");
                    return false;
                }

                if (_profileManager.SetPriority(profile) is false)
                {
                    Plugin.Log.Warning("[CustomizePlusIpc] Failed to set priority");
                    return false;
                }

                if (_profileManager.SetEnabled(profile) is false)
                {
                    Plugin.Log.Warning("[CustomizePlusIpc] Failed to set enabled");
                    return false;
                }

                return true;
            }).ConfigureAwait(false);
        
        Plugin.Log.Warning("[CustomizePlusIpc] Failed to apply customize profile because api is not available");
        return false;
    }

    /// <summary>
    ///     Removes the CustomizePlus profile created by AR
    /// </summary>
    public async Task<bool> DeleteCustomize()
    {
        if (ApiAvailable)
            return await Plugin.RunOnFramework(() => _profileManager.Delete()).ConfigureAwait(false);

        Plugin.Log.Warning("[CustomizePlusIpc] Failed to apply customize profile because api is not available");
        return false;
    }

    /// <summary>
    ///     Gets all active templates on provided player name
    /// </summary>
    /// <returns></returns>
    public async Task<IList?> GetActiveTemplates(string characterName)
    {
        if (ApiAvailable)
            return await Plugin.RunOnFramework(() => _profileManager.GetActiveProfileOnCharacter(characterName)).ConfigureAwait(false);
        
        Plugin.Log.Warning("[CustomizePlusIpc] Failed to get active templates because api is not available");
        return null;
    }

    /// <summary>
    ///     Converts a list of templates into a String64
    /// </summary>
    /// <param name="templates">List of templates retrieved from <see cref="GetActiveTemplates"/></param>
    /// <returns></returns>
    public async Task<string?> SerializeTemplates(IList templates)
    {
        if (ApiAvailable)
            return await Plugin.RunOnFramework(() => _templateManager.SerializeList(templates)).ConfigureAwait(false);
        
        Plugin.Log.Warning("[CustomizePlusIpc] Failed to serialize templates because api is not available");
        return null;
    }
    
    /// <summary>
    ///     Converts a string64 into a list of templates
    /// </summary>
    /// <param name="string64">Previously serialized list of templates retrieved from <see cref="GetActiveTemplates"/></param>
    /// <returns></returns>
    public async Task<IList?> DeserializeTemplates(string string64)
    {
        if (ApiAvailable)
            return await Plugin.RunOnFramework(() => _templateManager.DeserializeList(string64)).ConfigureAwait(false);
        
        Plugin.Log.Warning("[CustomizePlusIpc] Failed to deserialize templates because api is not available");
        return null;
    }

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
    
    // Updating Customize+ while the plugin is active will cause the assembly reference to become stale
    private void PluginInterfaceOnActivePluginsChanged(IActivePluginsChangedEventArgs args)
    {
        TestIpcAvailabilityAsync();
    }

    public void Dispose()
    {
        _profileManager.Delete();
        Plugin.PluginInterface.ActivePluginsChanged -= PluginInterfaceOnActivePluginsChanged;
        GC.SuppressFinalize(this);
    }
}