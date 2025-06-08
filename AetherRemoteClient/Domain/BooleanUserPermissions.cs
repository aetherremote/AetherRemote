using AetherRemoteCommon.Domain;
using AetherRemoteCommon.Domain.Enums;

namespace AetherRemoteClient.Domain;

/// <summary>
///     An object containing the content of <see cref="PrimaryPermissions"/> and <see cref="LinkshellPermissions"/> 
///     for use with ImGui Ui objects
/// </summary>
public class BooleanUserPermissions
{
    // Speak Permissions
    public bool Speak, Emote, Say, Yell, Shout, Tell, Party, Alliance, FreeCompany, PvPTeam, Echo, ChatEmote;
    
    // Misc Permissions
    public bool Customization, Equipment, Mods, BodySwap, Twinning, Moodles, CustomizePlus, Hypnosis;
    
    // Linkshell Permissions
    public bool Ls1, Ls2, Ls3, Ls4, Ls5, Ls6, Ls7, Ls8, Cwl1, Cwl2, Cwl3, Cwl4, Cwl5, Cwl6, Cwl7, Cwl8;

    /// <summary>
    ///     Checks to see if the values of one <see cref="BooleanUserPermissions"/> is equal to another
    /// </summary>
    public bool Equals(BooleanUserPermissions other)
    {
        // Speak Permissions
        if (Speak != other.Speak) return false;
        if (Emote != other.Emote) return false;
        if (Say != other.Say) return false;
        if (Yell != other.Yell) return false;
        if (Shout != other.Shout) return false;
        if (Tell != other.Tell) return false;
        if (Party != other.Party) return false;
        if (Alliance != other.Alliance) return false;
        if (FreeCompany != other.FreeCompany) return false;
        if (PvPTeam != other.PvPTeam) return false;
        if (Echo != other.Echo) return false;
        if (ChatEmote != other.ChatEmote) return false;
        
        // Misc Permissions
        if (Customization != other.Customization) return false;
        if (Equipment != other.Equipment) return false;
        if (Mods != other.Mods) return false;
        if (BodySwap != other.BodySwap) return false;
        if (Twinning != other.Twinning) return false;
        if (Moodles != other.Moodles) return false;
        if (CustomizePlus != other.CustomizePlus) return false;
        if (Hypnosis != other.Hypnosis) return false;
        
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
        if (Cwl8 != other.Cwl8) return false;
        
        return true;
    }

    /// <summary>
    ///     Converts a <see cref="UserPermissions"/> into a <see cref="BooleanUserPermissions"/>
    /// </summary>
    public static BooleanUserPermissions From(UserPermissions permissions)
    {
        return new BooleanUserPermissions
        {
            // Speak Permissions
            Speak = (permissions.Primary & PrimaryPermissions.Speak) == PrimaryPermissions.Speak,
            Emote = (permissions.Primary & PrimaryPermissions.Emote) == PrimaryPermissions.Emote,
            Say = (permissions.Primary & PrimaryPermissions.Say) == PrimaryPermissions.Say,
            Yell = (permissions.Primary & PrimaryPermissions.Yell) == PrimaryPermissions.Yell,
            Shout = (permissions.Primary & PrimaryPermissions.Shout) == PrimaryPermissions.Shout,
            Tell = (permissions.Primary & PrimaryPermissions.Tell) == PrimaryPermissions.Tell,
            Party = (permissions.Primary & PrimaryPermissions.Party) == PrimaryPermissions.Party,
            Alliance = (permissions.Primary & PrimaryPermissions.Alliance) == PrimaryPermissions.Alliance,
            FreeCompany = (permissions.Primary & PrimaryPermissions.FreeCompany) == PrimaryPermissions.FreeCompany,
            PvPTeam = (permissions.Primary & PrimaryPermissions.PvPTeam) == PrimaryPermissions.PvPTeam,
            Echo = (permissions.Primary & PrimaryPermissions.Echo) == PrimaryPermissions.Echo,
            ChatEmote = (permissions.Primary & PrimaryPermissions.ChatEmote) == PrimaryPermissions.ChatEmote,
            
            // Misc Permissions
            Customization = (permissions.Primary & PrimaryPermissions.Customization) == PrimaryPermissions.Customization,
            Equipment = (permissions.Primary & PrimaryPermissions.Equipment) == PrimaryPermissions.Equipment,
            Mods = (permissions.Primary & PrimaryPermissions.Mods) == PrimaryPermissions.Mods,
            BodySwap = (permissions.Primary & PrimaryPermissions.BodySwap) == PrimaryPermissions.BodySwap,
            Twinning = (permissions.Primary & PrimaryPermissions.Twinning) == PrimaryPermissions.Twinning,
            Moodles = (permissions.Primary & PrimaryPermissions.Moodles) == PrimaryPermissions.Moodles,
            CustomizePlus = (permissions.Primary & PrimaryPermissions.Customize) == PrimaryPermissions.Customize,
            Hypnosis = (permissions.Primary & PrimaryPermissions.Hypnosis) == PrimaryPermissions.Hypnosis,
            
            // Linkshell Permissions
            Ls1 = (permissions.Speak & LinkshellPermissions.Ls1) == LinkshellPermissions.Ls1,
            Ls2 = (permissions.Speak & LinkshellPermissions.Ls2) == LinkshellPermissions.Ls2,
            Ls3 = (permissions.Speak & LinkshellPermissions.Ls3) == LinkshellPermissions.Ls3,
            Ls4 = (permissions.Speak & LinkshellPermissions.Ls4) == LinkshellPermissions.Ls4,
            Ls5 = (permissions.Speak & LinkshellPermissions.Ls5) == LinkshellPermissions.Ls5,
            Ls6 = (permissions.Speak & LinkshellPermissions.Ls6) == LinkshellPermissions.Ls6,
            Ls7 = (permissions.Speak & LinkshellPermissions.Ls7) == LinkshellPermissions.Ls7,
            Ls8 = (permissions.Speak & LinkshellPermissions.Ls8) == LinkshellPermissions.Ls8,
            Cwl1 = (permissions.Speak & LinkshellPermissions.Cwl1) == LinkshellPermissions.Cwl1,
            Cwl2 = (permissions.Speak & LinkshellPermissions.Cwl2) == LinkshellPermissions.Cwl2,
            Cwl3 = (permissions.Speak & LinkshellPermissions.Cwl3) == LinkshellPermissions.Cwl3,
            Cwl4 = (permissions.Speak & LinkshellPermissions.Cwl4) == LinkshellPermissions.Cwl4,
            Cwl5 = (permissions.Speak & LinkshellPermissions.Cwl5) == LinkshellPermissions.Cwl5,
            Cwl6 = (permissions.Speak & LinkshellPermissions.Cwl6) == LinkshellPermissions.Cwl6,
            Cwl7 = (permissions.Speak & LinkshellPermissions.Cwl7) == LinkshellPermissions.Cwl7,
            Cwl8 = (permissions.Speak & LinkshellPermissions.Cwl8) == LinkshellPermissions.Cwl8
        };
    }

    /// <summary>
    ///     Converts a <see cref="BooleanUserPermissions"/> into a <see cref="UserPermissions"/>
    /// </summary>
    public static UserPermissions To(BooleanUserPermissions permissions)
    {
        var primary = PrimaryPermissions.None;
        var linkshell = LinkshellPermissions.None;
        
        // Speak Permissions
        if (permissions.Speak) primary |= PrimaryPermissions.Speak;
        if (permissions.Emote) primary |= PrimaryPermissions.Emote;
        if (permissions.Say) primary |= PrimaryPermissions.Say;
        if (permissions.Yell) primary |= PrimaryPermissions.Yell;
        if (permissions.Shout) primary |= PrimaryPermissions.Shout;
        if (permissions.Tell) primary |= PrimaryPermissions.Tell;
        if (permissions.Party) primary |= PrimaryPermissions.Party;
        if (permissions.Alliance) primary |= PrimaryPermissions.Alliance;
        if (permissions.FreeCompany) primary |= PrimaryPermissions.FreeCompany;
        if (permissions.PvPTeam) primary |= PrimaryPermissions.PvPTeam;
        if (permissions.Echo) primary |= PrimaryPermissions.Echo;
        if (permissions.ChatEmote) primary |= PrimaryPermissions.ChatEmote;
        
        // Misc Permissions
        if (permissions.Customization) primary |= PrimaryPermissions.Customization;
        if (permissions.Equipment) primary |= PrimaryPermissions.Equipment;
        if (permissions.Mods) primary |= PrimaryPermissions.Mods;
        if (permissions.BodySwap) primary |= PrimaryPermissions.BodySwap;
        if (permissions.Twinning) primary |= PrimaryPermissions.Twinning;
        if (permissions.Moodles) primary |= PrimaryPermissions.Moodles;
        if (permissions.CustomizePlus) primary |= PrimaryPermissions.Customize;
        if (permissions.Hypnosis) primary |= PrimaryPermissions.Hypnosis;
        
        // Linkshell Permissions
        if (permissions.Ls1) linkshell |= LinkshellPermissions.Ls1;
        if (permissions.Ls2) linkshell |= LinkshellPermissions.Ls2;
        if (permissions.Ls3) linkshell |= LinkshellPermissions.Ls3;
        if (permissions.Ls4) linkshell |= LinkshellPermissions.Ls4;
        if (permissions.Ls5) linkshell |= LinkshellPermissions.Ls5;
        if (permissions.Ls6) linkshell |= LinkshellPermissions.Ls6;
        if (permissions.Ls7) linkshell |= LinkshellPermissions.Ls7;
        if (permissions.Ls8) linkshell |= LinkshellPermissions.Ls8;
        if (permissions.Cwl1) linkshell |= LinkshellPermissions.Cwl1;
        if (permissions.Cwl2) linkshell |= LinkshellPermissions.Cwl2;
        if (permissions.Cwl3) linkshell |= LinkshellPermissions.Cwl3;
        if (permissions.Cwl4) linkshell |= LinkshellPermissions.Cwl4;
        if (permissions.Cwl5) linkshell |= LinkshellPermissions.Cwl5;
        if (permissions.Cwl6) linkshell |= LinkshellPermissions.Cwl6;
        if (permissions.Cwl7) linkshell |= LinkshellPermissions.Cwl7;
        if (permissions.Cwl8) linkshell |= LinkshellPermissions.Cwl8;

        return new UserPermissions { Primary = primary, Speak = linkshell };
    }
}