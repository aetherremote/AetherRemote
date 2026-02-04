using System.Linq;
using AetherRemoteClient.UI.Views.Friends.Ui;
using AetherRemoteCommon.Domain;
using AetherRemoteCommon.Domain.Enums.Permissions;

namespace AetherRemoteClient.Domain;

/// <summary>
///     A permission set for global permissions for use in <see cref="FriendsViewUi"/>
/// </summary>
public class GlobalPermissions
{
    // Primary Permissions
    public bool BodySwapValue;
    public bool CustomizePlusValue;
    public bool EmoteValue;
    public bool GlamourerCustomizationsValue;
    public bool GlamourerEquipmentValue;
    public bool HonorificValue;
    public bool HypnosisValue;
    public bool MoodlesValue;
    public bool PenumbraModsValue;
    public bool TwinningValue;
    
    // Speak Permissions
    public bool AllianceValue;
    public bool EchoValue;
    public bool FreeCompanyValue;
    public bool PartyValue;
    public bool PvPTeamValue;
    public bool RoleplayValue;
    public bool SayValue;
    public bool ShoutValue;
    public bool TellValue;
    public bool YellValue;

    public readonly bool[] LinkshellValues = new bool[8];
    public readonly bool[] CrossWorldLinkshellValues = new bool[8];
    
    // Elevated Permissions
    public bool PermanentTransformationValue;
    public bool PossessionValue;

    /// <summary>
    ///     Tests if one GlobalPermission is equal to another
    /// </summary>
    public bool IsEqualTo(GlobalPermissions? other)
    {
        if (other is null)
            return false;

        return 
            // Primary Permissions
            BodySwapValue == other.BodySwapValue &&
            CustomizePlusValue == other.CustomizePlusValue &&
            EmoteValue == other.EmoteValue &&
            GlamourerCustomizationsValue == other.GlamourerCustomizationsValue &&
            GlamourerEquipmentValue == other.GlamourerEquipmentValue &&
            HonorificValue == other.HonorificValue &&
            HypnosisValue == other.HypnosisValue &&
            MoodlesValue == other.MoodlesValue &&
            PenumbraModsValue == other.PenumbraModsValue &&
            TwinningValue == other.TwinningValue &&

            // Speak Permissions
            AllianceValue == other.AllianceValue &&
            EchoValue == other.EchoValue &&
            FreeCompanyValue == other.FreeCompanyValue &&
            PartyValue == other.PartyValue &&
            PvPTeamValue == other.PvPTeamValue &&
            RoleplayValue == other.RoleplayValue &&
            SayValue == other.SayValue &&
            ShoutValue == other.ShoutValue &&
            TellValue == other.TellValue &&
            YellValue == other.YellValue &&

            // Linkshells
            LinkshellValues.SequenceEqual(other.LinkshellValues) &&
            CrossWorldLinkshellValues.SequenceEqual(other.CrossWorldLinkshellValues) &&

            // Elevated Permissions
            PermanentTransformationValue == other.PermanentTransformationValue &&
            PossessionValue == other.PossessionValue;
    }
    
    /// <summary>
    ///     TODO
    /// </summary>
    public static GlobalPermissions From(ResolvedPermissions permissions)
    {
        return new GlobalPermissions
        {
            // Primary Permissions
            BodySwapValue = (permissions.Primary & PrimaryPermissions2.BodySwap) == PrimaryPermissions2.BodySwap,
            CustomizePlusValue = (permissions.Primary & PrimaryPermissions2.CustomizePlus) == PrimaryPermissions2.CustomizePlus,
            EmoteValue = (permissions.Primary & PrimaryPermissions2.Emote) == PrimaryPermissions2.Emote,
            GlamourerCustomizationsValue = (permissions.Primary & PrimaryPermissions2.GlamourerCustomization) == PrimaryPermissions2.GlamourerCustomization,
            GlamourerEquipmentValue = (permissions.Primary & PrimaryPermissions2.GlamourerEquipment) == PrimaryPermissions2.GlamourerEquipment,
            HonorificValue = (permissions.Primary & PrimaryPermissions2.Honorific) == PrimaryPermissions2.Honorific,
            HypnosisValue = (permissions.Primary & PrimaryPermissions2.Hypnosis) == PrimaryPermissions2.Hypnosis,
            MoodlesValue = (permissions.Primary & PrimaryPermissions2.Moodles) == PrimaryPermissions2.Moodles,
            PenumbraModsValue = (permissions.Primary & PrimaryPermissions2.Mods) == PrimaryPermissions2.Mods,
            TwinningValue = (permissions.Primary & PrimaryPermissions2.Twinning) == PrimaryPermissions2.Twinning,

            // Speak Permissions
            AllianceValue = (permissions.Speak & SpeakPermissions2.Alliance) == SpeakPermissions2.Alliance,
            EchoValue = (permissions.Speak & SpeakPermissions2.Echo) == SpeakPermissions2.Echo,
            FreeCompanyValue = (permissions.Speak & SpeakPermissions2.FreeCompany) == SpeakPermissions2.FreeCompany,
            PartyValue = (permissions.Speak & SpeakPermissions2.Party) == SpeakPermissions2.Party,
            PvPTeamValue = (permissions.Speak & SpeakPermissions2.PvPTeam) == SpeakPermissions2.PvPTeam,
            RoleplayValue = (permissions.Speak & SpeakPermissions2.Roleplay) == SpeakPermissions2.Roleplay,
            SayValue = (permissions.Speak & SpeakPermissions2.Say) == SpeakPermissions2.Say,
            ShoutValue = (permissions.Speak & SpeakPermissions2.Shout) == SpeakPermissions2.Shout,
            TellValue = (permissions.Speak & SpeakPermissions2.Tell) == SpeakPermissions2.Tell,
            YellValue = (permissions.Speak & SpeakPermissions2.Yell) == SpeakPermissions2.Yell,
            
            // Linkshells
            LinkshellValues =
            {
                [0] = (permissions.Speak & SpeakPermissions2.Ls1) == SpeakPermissions2.Ls1,
                [1] = (permissions.Speak & SpeakPermissions2.Ls2) == SpeakPermissions2.Ls2,
                [2] = (permissions.Speak & SpeakPermissions2.Ls3) == SpeakPermissions2.Ls3,
                [3] = (permissions.Speak & SpeakPermissions2.Ls4) == SpeakPermissions2.Ls4,
                [4] = (permissions.Speak & SpeakPermissions2.Ls5) == SpeakPermissions2.Ls5,
                [5] = (permissions.Speak & SpeakPermissions2.Ls6) == SpeakPermissions2.Ls6,
                [6] = (permissions.Speak & SpeakPermissions2.Ls7) == SpeakPermissions2.Ls7,
                [7] = (permissions.Speak & SpeakPermissions2.Ls8) == SpeakPermissions2.Ls8
            },
            
            // Cross-World Linkshells
            CrossWorldLinkshellValues =
            {
                [0] = (permissions.Speak & SpeakPermissions2.Cwl1) == SpeakPermissions2.Cwl1,
                [1] = (permissions.Speak & SpeakPermissions2.Cwl2) == SpeakPermissions2.Cwl2,
                [2] = (permissions.Speak & SpeakPermissions2.Cwl3) == SpeakPermissions2.Cwl3,
                [3] = (permissions.Speak & SpeakPermissions2.Cwl4) == SpeakPermissions2.Cwl4,
                [4] = (permissions.Speak & SpeakPermissions2.Cwl5) == SpeakPermissions2.Cwl5,
                [5] = (permissions.Speak & SpeakPermissions2.Cwl6) == SpeakPermissions2.Cwl6,
                [6] = (permissions.Speak & SpeakPermissions2.Cwl7) == SpeakPermissions2.Cwl7,
                [7] = (permissions.Speak & SpeakPermissions2.Cwl8) == SpeakPermissions2.Cwl8
            },
            
            // Elevated Permissions
            PermanentTransformationValue = (permissions.Elevated & ElevatedPermissions.PermanentTransformation) == ElevatedPermissions.PermanentTransformation,
            PossessionValue = (permissions.Elevated & ElevatedPermissions.Possession) == ElevatedPermissions.Possession
        };
    }

    /// <summary>
    ///     TODO
    /// </summary>
    public static ResolvedPermissions To(GlobalPermissions permissions)
    {
        // Primary Permissions
        var primary = PrimaryPermissions2.None;
        if (permissions.BodySwapValue) primary |= PrimaryPermissions2.BodySwap;
        if (permissions.CustomizePlusValue) primary |= PrimaryPermissions2.CustomizePlus;
        if (permissions.EmoteValue) primary |= PrimaryPermissions2.Emote;
        if (permissions.GlamourerCustomizationsValue) primary |= PrimaryPermissions2.GlamourerCustomization;
        if (permissions.GlamourerEquipmentValue) primary |= PrimaryPermissions2.GlamourerEquipment;
        if (permissions.HonorificValue) primary |= PrimaryPermissions2.Honorific;
        if (permissions.HypnosisValue) primary |= PrimaryPermissions2.Hypnosis;
        if (permissions.MoodlesValue) primary |= PrimaryPermissions2.Moodles;
        if (permissions.PenumbraModsValue) primary |= PrimaryPermissions2.Mods;
        if (permissions.TwinningValue) primary |= PrimaryPermissions2.Twinning;

        // Speak Permissions
        var speak = SpeakPermissions2.None;
        if (permissions.AllianceValue) speak |= SpeakPermissions2.Alliance;
        if (permissions.EchoValue) speak |= SpeakPermissions2.Echo;
        if (permissions.FreeCompanyValue) speak |= SpeakPermissions2.FreeCompany;
        if (permissions.PartyValue) speak |= SpeakPermissions2.Party;
        if (permissions.PvPTeamValue) speak |= SpeakPermissions2.PvPTeam;
        if (permissions.RoleplayValue) speak |= SpeakPermissions2.Roleplay;
        if (permissions.SayValue) speak |= SpeakPermissions2.Say;
        if (permissions.ShoutValue) speak |= SpeakPermissions2.Shout;
        if (permissions.TellValue) speak |= SpeakPermissions2.Tell;
        if (permissions.YellValue) speak |= SpeakPermissions2.Yell;

        // Linkshells
        if (permissions.LinkshellValues[0]) speak |= SpeakPermissions2.Ls1;
        if (permissions.LinkshellValues[1]) speak |= SpeakPermissions2.Ls2;
        if (permissions.LinkshellValues[2]) speak |= SpeakPermissions2.Ls3;
        if (permissions.LinkshellValues[3]) speak |= SpeakPermissions2.Ls4;
        if (permissions.LinkshellValues[4]) speak |= SpeakPermissions2.Ls5;
        if (permissions.LinkshellValues[5]) speak |= SpeakPermissions2.Ls6;
        if (permissions.LinkshellValues[6]) speak |= SpeakPermissions2.Ls7;
        if (permissions.LinkshellValues[7]) speak |= SpeakPermissions2.Ls8;

        // Cross-world Linkshells
        if (permissions.CrossWorldLinkshellValues[0]) speak |= SpeakPermissions2.Cwl1;
        if (permissions.CrossWorldLinkshellValues[1]) speak |= SpeakPermissions2.Cwl2;
        if (permissions.CrossWorldLinkshellValues[2]) speak |= SpeakPermissions2.Cwl3;
        if (permissions.CrossWorldLinkshellValues[3]) speak |= SpeakPermissions2.Cwl4;
        if (permissions.CrossWorldLinkshellValues[4]) speak |= SpeakPermissions2.Cwl5;
        if (permissions.CrossWorldLinkshellValues[5]) speak |= SpeakPermissions2.Cwl6;
        if (permissions.CrossWorldLinkshellValues[6]) speak |= SpeakPermissions2.Cwl7;
        if (permissions.CrossWorldLinkshellValues[7]) speak |= SpeakPermissions2.Cwl8;

        // Elevated Permissions
        var elevated = ElevatedPermissions.None;
        if (permissions.PermanentTransformationValue) elevated |= ElevatedPermissions.PermanentTransformation;
        if (permissions.PossessionValue) elevated |= ElevatedPermissions.Possession;

        return new ResolvedPermissions(primary, speak, elevated);
    }
}