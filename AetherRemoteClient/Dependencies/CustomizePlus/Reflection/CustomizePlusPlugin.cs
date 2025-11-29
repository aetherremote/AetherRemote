using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using AetherRemoteClient.Dependencies.CustomizePlus.Reflection.Managers;

namespace AetherRemoteClient.Dependencies.CustomizePlus.Reflection;

/// <summary>
///     Domain encapsulation of a CustomizePlus plugin
/// </summary>
public class CustomizePlusPlugin
{
    public readonly ProfileManager ProfileManager;
    public readonly TemplateManager TemplateManager;
    
    private CustomizePlusPlugin(ProfileManager profileManager, TemplateManager templateManager)
    {
        ProfileManager = profileManager;
        TemplateManager = templateManager;
    }

    /// <summary>
    ///     Creates a new copy of the domain representation of the reflected CustomizePlus plugin
    /// </summary>
    public static CustomizePlusPlugin? Create()
    {
        if (TryGetPluginInstance() is not { } pluginInstance)
            return null;

        if (ProfileManager.Create(pluginInstance) is not { } profileManager)
            return null;
        
        return TemplateManager.Create(pluginInstance) is { } templateManager
            ? new CustomizePlusPlugin(profileManager, templateManager)
            : null;
    }

    /// <summary>
    ///     Attempts to get the reflected instance of CustomizePlus
    /// </summary>
    /// <returns>Reflected instance of CustomizePlus</returns>
    private static object? TryGetPluginInstance()
    {
        try
        {
            var dalamudAssemblyName = Assembly
                .GetExecutingAssembly()
                .GetReferencedAssemblies()
                .FirstOrDefault(assembly => assembly.Name?.Contains("Dalamud", StringComparison.OrdinalIgnoreCase) is true);

            if (dalamudAssemblyName is null)
            {
                Plugin.Log.Error("[CustomizePlusPlugin.Create] Dalamud assembly was not found");
                return null;
            }

            var dalamudAssembly = AppDomain
                .CurrentDomain
                .GetAssemblies()
                .FirstOrDefault(assembly => assembly.GetName().Name == dalamudAssemblyName.Name);

            if (dalamudAssembly is null)
            {
                Plugin.Log.Error("[CustomizePlusPlugin.Create] Dalamud assembly was not loaded");
                return null;
            }

            var assemblyTypes = dalamudAssembly.GetTypes();
            var pluginManagerType = assemblyTypes.FirstOrDefault(type => type.FullName?.Contains("PluginManager", StringComparison.OrdinalIgnoreCase) is true);
            var localPluginType = assemblyTypes.FirstOrDefault(type => type.FullName?.Contains("LocalPlugin", StringComparison.OrdinalIgnoreCase) is true);
            var serviceHelperType = assemblyTypes.FirstOrDefault(type => type.FullName?.Contains("ServiceHelper", StringComparison.OrdinalIgnoreCase) is true);

            if (pluginManagerType is null || localPluginType is null || serviceHelperType is null)
            {
                Plugin.Log.Error("[CustomizePlusPlugin.Create] One or more required Dalamud service types are missing");
                return null;
            }

            var instanceField = localPluginType.GetField("instance", BindingFlags.NonPublic | BindingFlags.Instance);
            var assemblyNameProperty = localPluginType.GetProperty("AssemblyName");
            var getAsServiceMethod = serviceHelperType.GetMethod("GetAsService");

            if (instanceField is null || assemblyNameProperty is null || getAsServiceMethod is null)
            {
                Plugin.Log.Error("[CustomizePlusPlugin.Create] One or more required reflected fields, properties, or methods are missing");
                return null;
            }

            if (getAsServiceMethod.Invoke(null, [pluginManagerType]) is not Type pluginManagerServiceType)
            {
                Plugin.Log.Error("[CustomizePlusPlugin.Create] Failed to resolve plugin manager service type");
                return null;
            }

            if (pluginManagerServiceType.GetMethod("GetNullable") is not { } getNullableMethod)
            {
                Plugin.Log.Error("[CustomizePlusPlugin.Create] Failed to resolve GetNullable method");
                return null;
            }

            var pluginManagerInstance = getNullableMethod.Invoke(null, [0]);
            var installedPluginsProperty = pluginManagerType.GetProperty("InstalledPlugins");
            if (installedPluginsProperty?.GetValue(pluginManagerInstance) is not IList installedPlugins)
            {
                Plugin.Log.Error("[CustomizePlusPlugin.Create] Failed to resolve list of installed plugins");
                return null;
            }

            foreach (var plugin in installedPlugins)
                if (assemblyNameProperty.GetValue(plugin) is AssemblyName assemblyName)
                    if (assemblyName.Name?.Contains("CustomizePlus", StringComparison.OrdinalIgnoreCase) ?? false)
                        if (instanceField.GetValue(plugin) is { } pluginInstance)
                            return pluginInstance;
            
            Plugin.Log.Verbose("[CustomizePlusPlugin.Create] Failed to find CustomizePlus plugin");
            return null;
        }
        catch (Exception e)
        {
            Plugin.Log.Error($"[CustomizePlusPlugin.Create] An error occurred while reflecting, {e}");
            return null;
        }
    }
}