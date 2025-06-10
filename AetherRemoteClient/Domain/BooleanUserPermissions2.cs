using AetherRemoteCommon.Domain;
using AetherRemoteCommon.Domain.Enums.New;

namespace AetherRemoteClient.Domain;

/// <summary>
///     TODO
/// </summary>
public class BooleanUserPermissions2
{
    // Primary Permissions
    public bool Emote, Customization, Equipment, Mods, BodySwap, Twinning, CustomizePlus, Moodles, Hypnosis;
    
    // Speak Permissions
    public bool Say, Yell, Shout, Tell, Party, Alliance, FreeCompany, PvPTeam, Echo, Roleplay;
    
    // Linkshell Permissions
    public bool Ls1, Ls2, Ls3, Ls4, Ls5, Ls6, Ls7, Ls8, Cwl1, Cwl2, Cwl3, Cwl4, Cwl5, Cwl6, Cwl7, Cwl8;

    /// <summary>
    ///     TODO
    /// </summary>
    public bool Equals(BooleanUserPermissions2 other)
    {
        // Primary Permissions
        if (Emote != other.Emote) return false;
        if (Customization != other.Customization) return false;
        if (Equipment != other.Equipment) return false;
        if (Mods != other.Mods) return false;
        if (BodySwap != other.BodySwap) return false;
        if (Twinning!= other.Twinning) return false;
        if (CustomizePlus != other.CustomizePlus) return false;
        if (Moodles != other.Moodles) return false;
        if (Hypnosis != other.Hypnosis) return false;
        
        // Speak Permissions
        if (Say != other.Say) return false;
        if (Yell != other.Yell) return false;
        if (Shout != other.Shout) return false;
        if (Tell != other.Tell) return false;
        if (Party != other.Party) return false;
        if (Alliance != other.Alliance) return false;
        if (FreeCompany != other.FreeCompany) return false;
        if (PvPTeam != other.PvPTeam) return false;
        if (Echo != other.Echo) return false;
        if (Roleplay != other.Roleplay) return false;
        
        // Linkshell Permissions
        if (Ls1 != other.Ls1) return false;
        if (Ls2 != other.Ls2) return false;
        if (Ls3 != other.Ls3) return false;
        if (Ls4 != other.Ls4) return false;
        if (Ls5 != other.Ls5) return false;
        if (Ls6 != other.Ls6) return false;
        if (Ls7 != other.Ls7) return false;
        if (Ls8 != other.Ls8) return false;
        if (Cwl1 != other.Cwl1) return false;
        if (Cwl2 != other.Cwl2) return false;
        if (Cwl3 != other.Cwl3) return false;
        if (Cwl4 != other.Cwl4) return false;
        if (Cwl5 != other.Cwl5) return false;
        if (Cwl6 != other.Cwl6) return false;
        if (Cwl7 != other.Cwl7) return false;
        return Cwl8 == other.Cwl8;
    }

    /// <summary>
    ///     TODO
    /// </summary>
    public static BooleanUserPermissions2 From(UserPermissions permissions)
    {
        return new BooleanUserPermissions2
        {
            // Primary
            Emote = (permissions.Primary & PrimaryPermissions2.Emote) == PrimaryPermissions2.Emote,
            Customization = (permissions.Primary & PrimaryPermissions2.GlamourerCustomization) ==  PrimaryPermissions2.GlamourerCustomization,
            Equipment = (permissions.Primary & PrimaryPermissions2.GlamourerEquipment) ==  PrimaryPermissions2.GlamourerEquipment,
            Mods = (permissions.Primary & PrimaryPermissions2.Mods) == PrimaryPermissions2.Mods,
            BodySwap = (permissions.Primary & PrimaryPermissions2.BodySwap) == PrimaryPermissions2.BodySwap,
            Twinning = (permissions.Primary & PrimaryPermissions2.Twinning) == PrimaryPermissions2.Twinning,
            CustomizePlus = (permissions.Primary & PrimaryPermissions2.CustomizePlus) == PrimaryPermissions2.CustomizePlus,
            Moodles = (permissions.Primary & PrimaryPermissions2.Moodles) == PrimaryPermissions2.Moodles,
            Hypnosis = (permissions.Primary & PrimaryPermissions2.Hypnosis) == PrimaryPermissions2.Hypnosis,
            
            // Speak Permissions
            Say = (permissions.Speak & SpeakPermissions2.Say) == SpeakPermissions2.Say,
            Yell = (permissions.Speak & SpeakPermissions2.Yell) == SpeakPermissions2.Yell,
            Shout = (permissions.Speak & SpeakPermissions2.Shout) == SpeakPermissions2.Shout,
            Tell = (permissions.Speak & SpeakPermissions2.Tell) == SpeakPermissions2.Tell,
            Party = (permissions.Speak & SpeakPermissions2.Party) == SpeakPermissions2.Party,
            Alliance = (permissions.Speak & SpeakPermissions2.Alliance) == SpeakPermissions2.Alliance,
            FreeCompany = (permissions.Speak & SpeakPermissions2.FreeCompany) == SpeakPermissions2.FreeCompany,
            PvPTeam = (permissions.Speak & SpeakPermissions2.PvPTeam) == SpeakPermissions2.PvPTeam,
            Echo = (permissions.Speak & SpeakPermissions2.Echo) == SpeakPermissions2.Echo,
            Roleplay = (permissions.Speak & SpeakPermissions2.Roleplay) == SpeakPermissions2.Roleplay,
            
            // Linkshell Permissions
            Ls1 = (permissions.Speak & SpeakPermissions2.Ls1) == SpeakPermissions2.Ls1,
            Ls2 = (permissions.Speak & SpeakPermissions2.Ls2) == SpeakPermissions2.Ls2,
            Ls3 = (permissions.Speak & SpeakPermissions2.Ls3) == SpeakPermissions2.Ls3,
            Ls4 = (permissions.Speak & SpeakPermissions2.Ls4) == SpeakPermissions2.Ls4,
            Ls5 = (permissions.Speak & SpeakPermissions2.Ls5) == SpeakPermissions2.Ls5,
            Ls6 = (permissions.Speak & SpeakPermissions2.Ls6) == SpeakPermissions2.Ls6,
            Ls7 = (permissions.Speak & SpeakPermissions2.Ls7) == SpeakPermissions2.Ls7,
            Ls8 = (permissions.Speak & SpeakPermissions2.Ls8) == SpeakPermissions2.Ls8,
            Cwl1 = (permissions.Speak & SpeakPermissions2.Cwl1) == SpeakPermissions2.Cwl1,
            Cwl2 = (permissions.Speak & SpeakPermissions2.Cwl2) == SpeakPermissions2.Cwl2,
            Cwl3 = (permissions.Speak & SpeakPermissions2.Cwl3) == SpeakPermissions2.Cwl3,
            Cwl4 = (permissions.Speak & SpeakPermissions2.Cwl4) == SpeakPermissions2.Cwl4,
            Cwl5 = (permissions.Speak & SpeakPermissions2.Cwl5) == SpeakPermissions2.Cwl5,
            Cwl6 = (permissions.Speak & SpeakPermissions2.Cwl6) == SpeakPermissions2.Cwl6,
            Cwl7 = (permissions.Speak & SpeakPermissions2.Cwl7) == SpeakPermissions2.Cwl7,
            Cwl8 = (permissions.Speak & SpeakPermissions2.Cwl8) == SpeakPermissions2.Cwl8
        };
    }

    /// <summary>
    ///     TODO
    /// </summary>
    public static UserPermissions To(BooleanUserPermissions2 permissions)
    {
        // Initialization
        var primary = PrimaryPermissions2.None;
        var speak =  SpeakPermissions2.None;
        
        // Primary
        if (permissions.Emote) primary |= PrimaryPermissions2.Emote;
        if (permissions.Customization) primary |= PrimaryPermissions2.GlamourerCustomization;
        if (permissions.Equipment) primary |= PrimaryPermissions2.GlamourerEquipment;
        if (permissions.Mods) primary |= PrimaryPermissions2.Mods;
        if (permissions.BodySwap) primary |= PrimaryPermissions2.BodySwap;
        if (permissions.Twinning) primary |= PrimaryPermissions2.Twinning;
        if (permissions.CustomizePlus) primary |= PrimaryPermissions2.CustomizePlus;
        if (permissions.Moodles) primary |= PrimaryPermissions2.Moodles;
        if (permissions.Hypnosis) primary |= PrimaryPermissions2.Hypnosis;

        // Speak Permissions
        if (permissions.Say) speak |= SpeakPermissions2.Say;
        if (permissions.Yell) speak |= SpeakPermissions2.Yell;
        if (permissions.Shout) speak |= SpeakPermissions2.Shout;
        if (permissions.Tell) speak |= SpeakPermissions2.Tell;
        if (permissions.Party) speak |= SpeakPermissions2.Party;
        if (permissions.Alliance) speak |= SpeakPermissions2.Alliance;
        if (permissions.FreeCompany) speak |= SpeakPermissions2.FreeCompany;
        if (permissions.PvPTeam) speak |= SpeakPermissions2.PvPTeam;
        if (permissions.Echo) speak |= SpeakPermissions2.Echo;
        if (permissions.Roleplay) speak |= SpeakPermissions2.Roleplay;

        // Linkshell Permissions
        if (permissions.Ls1) speak |= SpeakPermissions2.Ls1;
        if (permissions.Ls2) speak |= SpeakPermissions2.Ls2;
        if (permissions.Ls3) speak |= SpeakPermissions2.Ls3;
        if (permissions.Ls4) speak |= SpeakPermissions2.Ls4;
        if (permissions.Ls5) speak |= SpeakPermissions2.Ls5;
        if (permissions.Ls6) speak |= SpeakPermissions2.Ls6;
        if (permissions.Ls7) speak |= SpeakPermissions2.Ls7;
        if (permissions.Ls8) speak |= SpeakPermissions2.Ls8;
        if (permissions.Cwl1) speak |= SpeakPermissions2.Cwl1;
        if (permissions.Cwl2) speak |= SpeakPermissions2.Cwl2;
        if (permissions.Cwl3) speak |= SpeakPermissions2.Cwl3;
        if (permissions.Cwl4) speak |= SpeakPermissions2.Cwl4;
        if (permissions.Cwl5) speak |= SpeakPermissions2.Cwl5;
        if (permissions.Cwl6) speak |= SpeakPermissions2.Cwl6;
        if (permissions.Cwl7) speak |= SpeakPermissions2.Cwl7;
        if (permissions.Cwl8) speak |= SpeakPermissions2.Cwl8;
        return new UserPermissions(primary, speak);
    }
}