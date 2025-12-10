using System;
using System.Globalization;
using AetherRemoteClient.Domain.Dependencies.Glamourer;
using Newtonsoft.Json.Linq;

namespace AetherRemoteClient.Utils;

public static class GlamourerDesignHelper
{
    // TODO: Handle exceptions when permanent TFs come back
    public static GlamourerEquipmentSlot ToEquipmentSlot(string key)
    {
        var parsed = uint.Parse(key, NumberStyles.HexNumber);
        var index = (byte)(parsed >> 16) & 0xFF;
        return (GlamourerEquipmentSlot)(1 << index);
    }
    
    // TODO: Handle exceptions when permanent TFs come back
    public static JObject ToJObject(GlamourerDesign design)
    {
        // Convert to a JToken
        var json = JToken.FromObject(design);
        
        // Create an empty mods array
        json["Mods"] = new JArray();
        
        // Creates a link object with two empty arrays
        json["Links"] = new JObject
        {
            ["Before"] = new JArray(),
            ["After"] = new JArray()
        };
        
        // Return the object as a JObject
        return json as JObject ?? new JObject();
    }
    
    public static GlamourerDesign? FromJObject(JObject? design)
    {
        try
        {
            // Reject null objects
            if (design is null)
                return null;
        
            // Copy
            var copy = design.DeepClone();
        
            // Remove Mods & Links
            copy["Mods"]?.Parent?.Remove();
            copy["Links"]?.Parent?.Remove();
        
            // Create a new domain object
            return copy.ToObject<GlamourerDesign>();
        }
        catch (Exception e)
        {
            Plugin.Log.Warning($"[GlamourerDesignHelper.FromJObject] Unexpected error, {e}");
            return null;
        }
    }
}