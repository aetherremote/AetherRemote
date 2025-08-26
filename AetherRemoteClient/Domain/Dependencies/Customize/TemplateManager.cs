using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AetherRemoteClient.Domain.Dependencies.Customize;

/// <summary>
///     Provides access to the Customize template manager via reflection
/// </summary>
public class TemplateManager(object pluginInstance)
{
    private Type? _templateType;
    private MethodInfo? _importFromBase64Method;
    private MethodInfo? _jsonSerializeMethod;
    private MethodInfo? _jsonDeserializeMethod;

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
            _templateType = assembly.GetType("CustomizePlus.Templates.Data.Template");
            _importFromBase64Method = deserializationType?.GetMethod("ImportFromBase64", BindingFlags.Public | BindingFlags.Static);
            _jsonSerializeMethod = _templateType?.GetMethod("JsonSerialize", BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly);
            _jsonDeserializeMethod = _templateType?.GetMethod("Load", BindingFlags.Static | BindingFlags.Public);
            
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
        if (_importFromBase64Method?.Invoke(null, parameters) is not { } version)
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

    /// <summary>
    ///     Converts a list of Customize Plus templates into a Base64 string
    /// </summary>
    /// <param name="templates"></param>
    /// <returns>Base64 string representing the template</returns>
    public string? SerializeList(IList templates)
    {
        if (_jsonSerializeMethod is null)
        {
            Plugin.Log.Error("[TemplateManager] Unable to serialize list because serialize method was not found");
            return null;
        }

        var result = new JArray();
        foreach (var template in templates)
        {
            if (template is null || template.GetType() != _templateType)
            {
                Plugin.Log.Warning($"[TemplateManager] Template was null or not an expected template {template}");
                continue;
            }

            try
            {
                if (_jsonSerializeMethod.Invoke(template, null) is not JObject json)
                {
                    Plugin.Log.Warning($"[TemplateManager] Unable to serialize template {template}");
                    continue;
                }
            
                result.Add(json);
            }
            catch (Exception e)
            {
                Plugin.Log.Warning($"[TemplateManager] Unknown error serializing template {template}, {e}");
            }
        }

        var raw = result.ToString(Formatting.None);
        var bytes = Encoding.UTF8.GetBytes(raw);
        return Convert.ToBase64String(bytes);
    }

    /// <summary>
    ///     Converts a Base64 string into a list of Customize Plus templates
    /// </summary>
    /// <param name="templatesBase64"></param>
    /// <returns></returns>
    public IList? DeserializeList(string templatesBase64)
    {
        if (_jsonDeserializeMethod is null)
        {
            Plugin.Log.Error("[TemplateManager] Unable to deserialize string because deserialize method was not found");
            return null;
        }
        
        var bytes = Convert.FromBase64String(templatesBase64);
        var raw =  Encoding.UTF8.GetString(bytes);
        var array = JArray.Parse(raw);
        
        var result = new List<object>();
        foreach (var token in array)
        {
            if (token is not JObject json)
            {
                Plugin.Log.Warning($"[TemplateManager] Token was null or not a JObject {token}");
                continue;
            }

            try
            {
                var args = new object[] { json };
                if (_jsonDeserializeMethod.Invoke(null, args) is not { } template)
                {
                    Plugin.Log.Warning($"[TemplateManager] Deserialization returned null for {args}");
                    continue;
                }
                
                result.Add(template);
            }
            catch (Exception e)
            {
                Plugin.Log.Warning($"[TemplateManager] Unknown error deserializing template {json}, {e}");
            }
        }
        
        return result;
    }

    public IList? DeserializeTemplates(string templateData)
    {
        try
        {
            if (_templateType is null)
            {
                Plugin.Log.Warning("[TemplateManager] Unable to convert list to template type because type was not loaded");
                return null;
            }
        
            var result = (IList?)Activator.CreateInstance(typeof(List<>).MakeGenericType(_templateType));
            if (result is null)
            {
                Plugin.Log.Warning("[TemplateManager] Failed to create generic list for reflected type");
                return null;
            }
            
            var type = typeof(List<>).MakeGenericType(_templateType);
            var deserialized = JsonConvert.DeserializeObject(templateData, type);
            if (deserialized is not null)
                return (IList)deserialized;
            
            Plugin.Log.Warning("[TemplateManager] Failed to deserialize template data");
            return null;
        }
        catch (Exception e)
        {
            Plugin.Log.Warning($"[TemplateManager] Unexpected error occurred: {e}");
            return null;
        }
    }

    /// <summary>
    ///     Converts a generic List[Object] to the internal reflected customize List[Template]
    /// </summary>
    /// <returns></returns>
    public IList? ConvertToTemplateType(IList deserialized)
    {
        try
        {
            if (_templateType is null)
            {
                Plugin.Log.Warning("[TemplateManager] Unable to convert list to template type because type was not loaded");
                return null;
            }
        
            var result = (IList?)Activator.CreateInstance(typeof(List<>).MakeGenericType(_templateType));
            if (result is null)
            {
                Plugin.Log.Warning("[TemplateManager] Failed to create generic list for reflected type");
                return null;
            }

            foreach (var template in deserialized)
            {
                var json = JsonConvert.SerializeObject(template);
                var converted = JsonConvert.DeserializeObject(json, _templateType);
                result.Add(converted);
            }
        
            return result;
        }
        catch (Exception e)
        {
            Plugin.Log.Warning($"[TemplateManager] Unexpected error occurred: {e}");
            return null;
        }
    }
}