using System;
using System.Reflection;
using Newtonsoft.Json;

namespace AetherRemoteClient.Ipc.Domain;

/// <summary>
///     Provides access to the Customize template manager via reflection
/// </summary>
public class TemplateManager(object pluginInstance)
{
    private Type? _templateType;
    private MethodInfo? _deserializeMethod;

    /// <summary>
    ///     Initialize the manager. This must be called before calling any function on this manager
    /// </summary>
    /// <returns>True is successful, otherwise false</returns>
    public bool Initialize()
    {
        try
        {
            var pluginType = pluginInstance.GetType();
            var servicesFieldInfo = pluginType.GetField("_services", BindingFlags.Instance | BindingFlags.NonPublic);
            var servicesInstance = servicesFieldInfo?.GetValue(pluginInstance);
            var getServiceMethodInfo = servicesInstance?.GetType().GetMethod("GetService");

            var assembly = pluginType.Assembly;
            var templateManagerType = assembly.GetType("CustomizePlus.Templates.TemplateManager")!;
            var getTemplateManagerMethod = getServiceMethodInfo?.MakeGenericMethod(templateManagerType);
            var templateManagerInstance = getTemplateManagerMethod?.Invoke(servicesInstance, null);
            if (templateManagerInstance is null)
            {
                Plugin.Log.Fatal("Unable to get customize template manager instance");
                return false;
            }
            
            var deserializationType = assembly.GetType("CustomizePlus.Core.Helpers.Base64Helper");
            _deserializeMethod = deserializationType?.GetMethod("ImportFromBase64", BindingFlags.Public | BindingFlags.Static);
            _templateType = assembly.GetType("CustomizePlus.Templates.Data.Template");
            
            return true;
        }
        catch (Exception e)
        {
            Plugin.Log.Fatal($"Could not initialize TemplateManager, {e}");
            return false;
        }
    }
    
    /// <summary>
    ///     Creates a new Customize template from provided string
    /// </summary>
    /// <param name="templateData">Customize code as String64</param>
    /// <returns>The created template</returns>
    public object? CreateTemplate(string templateData)
    {
        object?[] parameters = [templateData, null];
        if (_deserializeMethod?.Invoke(null, parameters) is not { } version)
        {
            Plugin.Log.Warning("Unable to deserialize template");
            return null;
        }

        var integer = Convert.ToInt32(version);
        if (integer is not 4)
        {
            Plugin.Log.Warning("Unable to deserialize template, incompatible version");
            return null;
        }

        if (parameters[1] is not string json)
        {
            Plugin.Log.Warning("Unable to deserialize template");
            return null;
        }

        if (JsonConvert.DeserializeObject(json, _templateType!) is { } template)
            return template;
        
        Plugin.Log.Warning("Unable to deserialize template, aborting");
        return null;
    }
}