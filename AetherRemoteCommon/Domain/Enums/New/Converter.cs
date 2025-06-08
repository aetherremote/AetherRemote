namespace AetherRemoteCommon.Domain.Enums.New;

public static class Converter
{
    public static (PrimaryPermissions2, SpeakPermissions2) Convert(PrimaryPermissions primary, LinkshellPermissions linkshell)
    {
        var primary2 = PrimaryPermissions2.None;
        var speak2 = SpeakPermissions2.None;

        if ((primary & PrimaryPermissions.Emote) == PrimaryPermissions.Emote)
            primary2 |= PrimaryPermissions2.Emote;
        
        if ((primary & PrimaryPermissions.Say) == PrimaryPermissions.Say)
            speak2 |= SpeakPermissions2.Say;
        
        if ((primary & PrimaryPermissions.Yell) == PrimaryPermissions.Yell)
            speak2 |= SpeakPermissions2.Yell;
        
        if ((primary & PrimaryPermissions.Shout) == PrimaryPermissions.Shout)
            speak2 |= SpeakPermissions2.Shout;
        
        if ((primary & PrimaryPermissions.Tell) == PrimaryPermissions.Tell)
            speak2 |= SpeakPermissions2.Tell;
        
        if ((primary & PrimaryPermissions.Party) == PrimaryPermissions.Party)
            speak2 |= SpeakPermissions2.Party;
        
        if ((primary & PrimaryPermissions.Alliance) == PrimaryPermissions.Alliance)
            speak2 |= SpeakPermissions2.Alliance;
        
        if ((primary & PrimaryPermissions.FreeCompany) == PrimaryPermissions.FreeCompany)
            speak2 |= SpeakPermissions2.FreeCompany;
        
        if ((primary & PrimaryPermissions.PvPTeam) == PrimaryPermissions.PvPTeam)
            speak2 |= SpeakPermissions2.PvPTeam;
        
        if ((primary & PrimaryPermissions.Echo) == PrimaryPermissions.Echo)
            speak2 |= SpeakPermissions2.Echo;
        
        if ((primary & PrimaryPermissions.ChatEmote) == PrimaryPermissions.ChatEmote)
            speak2 |= SpeakPermissions2.Roleplay;
        
        if ((primary & PrimaryPermissions.Customization) == PrimaryPermissions.Customization)
            primary2 |= PrimaryPermissions2.GlamourerCustomization;
        
        if ((primary & PrimaryPermissions.Equipment) == PrimaryPermissions.Equipment)
            primary2 |= PrimaryPermissions2.GlamourerEquipment;
        
        if ((primary & PrimaryPermissions.Mods) == PrimaryPermissions.Mods)
            primary2 |= PrimaryPermissions2.Mods;
        
        if ((primary & PrimaryPermissions.BodySwap) == PrimaryPermissions.BodySwap)
            primary2 |= PrimaryPermissions2.BodySwap;
        
        if ((primary & PrimaryPermissions.Twinning) == PrimaryPermissions.Twinning)
            primary2 |= PrimaryPermissions2.Twinning;
        
        if ((primary & PrimaryPermissions.Customize) == PrimaryPermissions.Customize)
            primary2 |= PrimaryPermissions2.CustomizePlus;
        
        if ((primary & PrimaryPermissions.Moodles) == PrimaryPermissions.Moodles)
            primary2 |= PrimaryPermissions2.Moodles;
        
        if ((primary & PrimaryPermissions.Hypnosis) == PrimaryPermissions.Hypnosis)
            primary2 |= PrimaryPermissions2.Hypnosis;
        
        if ((linkshell & LinkshellPermissions.Ls1) == LinkshellPermissions.Ls1)
            speak2 |= SpeakPermissions2.Ls1;
        if ((linkshell & LinkshellPermissions.Ls2) == LinkshellPermissions.Ls2)
            speak2 |= SpeakPermissions2.Ls2;
        if ((linkshell & LinkshellPermissions.Ls3) == LinkshellPermissions.Ls3)
            speak2 |= SpeakPermissions2.Ls3;
        if ((linkshell & LinkshellPermissions.Ls4) == LinkshellPermissions.Ls4)
            speak2 |= SpeakPermissions2.Ls4;
        if ((linkshell & LinkshellPermissions.Ls5) == LinkshellPermissions.Ls5)
            speak2 |= SpeakPermissions2.Ls5;
        if ((linkshell & LinkshellPermissions.Ls6) == LinkshellPermissions.Ls6)
            speak2 |= SpeakPermissions2.Ls6;
        if ((linkshell & LinkshellPermissions.Ls7) == LinkshellPermissions.Ls7)
            speak2 |= SpeakPermissions2.Ls7;
        if ((linkshell & LinkshellPermissions.Ls8) == LinkshellPermissions.Ls8)
            speak2 |= SpeakPermissions2.Ls8;
        
        if ((linkshell & LinkshellPermissions.Cwl1) == LinkshellPermissions.Cwl1)
            speak2 |= SpeakPermissions2.Cwl1;
        if ((linkshell & LinkshellPermissions.Cwl2) == LinkshellPermissions.Cwl2)
            speak2 |= SpeakPermissions2.Cwl2;
        if ((linkshell & LinkshellPermissions.Cwl3) == LinkshellPermissions.Cwl3)
            speak2 |= SpeakPermissions2.Cwl3;
        if ((linkshell & LinkshellPermissions.Cwl4) == LinkshellPermissions.Cwl4)
            speak2 |= SpeakPermissions2.Cwl4;
        if ((linkshell & LinkshellPermissions.Cwl5) == LinkshellPermissions.Cwl5)
            speak2 |= SpeakPermissions2.Cwl5;
        if ((linkshell & LinkshellPermissions.Cwl6) == LinkshellPermissions.Cwl6)
            speak2 |= SpeakPermissions2.Cwl6;
        if ((linkshell & LinkshellPermissions.Cwl7) == LinkshellPermissions.Cwl7)
            speak2 |= SpeakPermissions2.Cwl7;
        if ((linkshell & LinkshellPermissions.Cwl8) == LinkshellPermissions.Cwl8)
            speak2 |= SpeakPermissions2.Cwl8;
        
        return (primary2, speak2);
    }
}