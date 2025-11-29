using System;
using AetherRemoteClient.Dependencies.CustomizePlus.Domain;
using AetherRemoteClient.Dependencies.CustomizePlus.Reflection.Domain;
using Newtonsoft.Json;

namespace AetherRemoteClient.Dependencies.CustomizePlus.Reflection.Managers;
/// <summary>
///     Domain encapsulation of a CustomizePlus TemplateManager class
/// </summary>
public class TemplateManager
{
    // Injected
    private readonly Type _templateType;
    
    /// <summary>
    ///     <inheritdoc cref="TemplateManager"/>
    /// </summary>
    private TemplateManager(Type templateType)
    {
        _templateType = templateType;
    }

    /// <summary>
    ///     Deserializes JSON into a CustomizePlus Template
    /// </summary>
    public Template? DeserializeTemplate(string json)
    {
        try
        {
            return JsonConvert.DeserializeObject(json, _templateType) is { } template
                ? new Template(template)
                : null;
        }
        catch (Exception e)
        {
            Plugin.Log.Error($"[TemplateManager.DeserializeTemplate] An error occurred, {e}");
            return null;
        }
    }
    
    /// <summary>
    ///     Creates a new instance of the TemplateManager
    /// </summary>
    /// <remarks>
    ///     Ideally this is called only a single time to not incur multiple reflection calls
    /// </remarks>
    public static TemplateManager? Create(object pluginInstance)
    {
        try
        {
            // Get Plugin Type
            var pluginType = pluginInstance.GetType();

            // Template Type
            return pluginType.Assembly.GetType("CustomizePlus.Templates.Data.Template") is { } template 
                ? new TemplateManager(template)
                : null;
        }
        catch (Exception e)
        {
            Plugin.Log.Error($"[TemplateManager.Create] An error occurred while reflecting, {e}");
            return null;
        }
    }
}