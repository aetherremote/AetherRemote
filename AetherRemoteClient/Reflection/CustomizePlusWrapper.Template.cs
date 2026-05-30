using System;
using Newtonsoft.Json;

namespace AetherRemoteClient.Reflection;

public partial class CustomizePlusWrapper
{
    /// <summary>
    ///     Wrapper for Customize Plus' templates
    /// </summary>
    private class CustomizePlusTemplateWrapper
    {
        // Delegates
        private readonly Func<string, object?> _deserializeTemplate;
    
        /// <summary>
        ///     <inheritdoc cref="CustomizePlusTemplateWrapper"/>
        /// </summary>
        private CustomizePlusTemplateWrapper(Func<string, object?> deserializeTemplate)
        {
            _deserializeTemplate = deserializeTemplate;
        }

        /// <summary>
        ///     Create a new wrapper for the customize plus templates.
        /// </summary>
        /// <remarks>This is only intended to be called by <see cref="Reflection.CustomizePlusWrapper.Wrap"/></remarks>
        public static CustomizePlusTemplateWrapper Wrap(Type templateType)
        {
            // Package all the delegates up in a nice little bow
            return new CustomizePlusTemplateWrapper(DeserializeTemplate);

            // Delegates (local function because Rider says so)
            object? DeserializeTemplate(string json) => JsonConvert.DeserializeObject(json, templateType);
        }
    
        /// <summary>
        ///     Returns the deserialized template from provided JSON
        /// </summary>
        public object? Deserialize(string json) => _deserializeTemplate(json);
    }
}
