using System;
using System.Collections.Generic;
using AetherRemoteClient.Domain.Dependencies.Glamourer.Components;

namespace AetherRemoteClient.Domain.Dependencies.Glamourer;

public class GlamourerDesign
{
    public int FileVersion;
    public Guid Identifier;
    public DateTimeOffset CreationDate;
    public DateTimeOffset LastEdit;
    public string Name = string.Empty;
    public string Description = string.Empty;
    public bool ForcedRedraw;
    public bool ResetAdvancedDyes;
    public bool ResetTemporarySettings;
    public string Color = string.Empty;
    public bool QuickDesign = true;
    public string[] Tags = [];
    public bool WriteProtected;
    public GlamourerEquipment Equipment = new();
    public GlamourerBonus Bonus = new();
    public GlamourerCustomize Customize = new();
    public GlamourerParameter Parameters = new();
    public Dictionary<string, GlamourerMaterial> Materials = [];

    public GlamourerDesign Clone()
    {
        // Clone Tags
        var tags = new string[Tags.Length];
        for (var i = 0; i < Tags.Length; i++)
            tags[i] = Tags[i];
        
        // Clone Materials
        var materials = new Dictionary<string, GlamourerMaterial>();
        foreach (var material in Materials)
            materials[material.Key] = material.Value.Clone();
        
        // Memberwise copy
        var copy = (GlamourerDesign)MemberwiseClone();
        
        // Add created fields
        copy.Tags = tags;
        copy.Materials = materials;
        
        // Add cloned fields
        copy.Equipment = Equipment.Clone();
        copy.Bonus = Bonus.Clone();
        copy.Customize = Customize.Clone();
        copy.Parameters = Parameters.Clone();
        
        // Return copy
        return copy;
    }
}