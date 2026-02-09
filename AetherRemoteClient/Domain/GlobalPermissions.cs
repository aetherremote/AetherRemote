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
            BodySwapValue = (permissions.Primary & PrimaryPermissions.BodySwap) == PrimaryPermissions.BodySwap,
            CustomizePlusValue = (permissions.Primary & PrimaryPermissions.CustomizePlus) == PrimaryPermissions.CustomizePlus,
            EmoteValue = (permissions.Primary & PrimaryPermissions.Emote) == PrimaryPermissions.Emote,
            GlamourerCustomizationsValue = (permissions.Primary & PrimaryPermissions.GlamourerCustomization) == PrimaryPermissions.GlamourerCustomization,
            GlamourerEquipmentValue = (permissions.Primary & PrimaryPermissions.GlamourerEquipment) == PrimaryPermissions.GlamourerEquipment,
            HonorificValue = (permissions.Primary & PrimaryPermissions.Honorific) == PrimaryPermissions.Honorific,
            HypnosisValue = (permissions.Primary & PrimaryPermissions.Hypnosis) == PrimaryPermissions.Hypnosis,
            MoodlesValue = (permissions.Primary & PrimaryPermissions.Moodles) == PrimaryPermissions.Moodles,
            PenumbraModsValue = (permissions.Primary & PrimaryPermissions.Mods) == PrimaryPermissions.Mods,
            TwinningValue = (permissions.Primary & PrimaryPermissions.Twinning) == PrimaryPermissions.Twinning,

            // Speak Permissions
            AllianceValue = (permissions.Speak & SpeakPermissions.Alliance) == SpeakPermissions.Alliance,
            EchoValue = (permissions.Speak & SpeakPermissions.Echo) == SpeakPermissions.Echo,
            FreeCompanyValue = (permissions.Speak & SpeakPermissions.FreeCompany) == SpeakPermissions.FreeCompany,
            PartyValue = (permissions.Speak & SpeakPermissions.Party) == SpeakPermissions.Party,
            PvPTeamValue = (permissions.Speak & SpeakPermissions.PvPTeam) == SpeakPermissions.PvPTeam,
            RoleplayValue = (permissions.Speak & SpeakPermissions.Roleplay) == SpeakPermissions.Roleplay,
            SayValue = (permissions.Speak & SpeakPermissions.Say) == SpeakPermissions.Say,
            ShoutValue = (permissions.Speak & SpeakPermissions.Shout) == SpeakPermissions.Shout,
            TellValue = (permissions.Speak & SpeakPermissions.Tell) == SpeakPermissions.Tell,
            YellValue = (permissions.Speak & SpeakPermissions.Yell) == SpeakPermissions.Yell,
            
            // Linkshells
            LinkshellValues =
            {
                [0] = (permissions.Speak & SpeakPermissions.Ls1) == SpeakPermissions.Ls1,
                [1] = (permissions.Speak & SpeakPermissions.Ls2) == SpeakPermissions.Ls2,
                [2] = (permissions.Speak & SpeakPermissions.Ls3) == SpeakPermissions.Ls3,
                [3] = (permissions.Speak & SpeakPermissions.Ls4) == SpeakPermissions.Ls4,
                [4] = (permissions.Speak & SpeakPermissions.Ls5) == SpeakPermissions.Ls5,
                [5] = (permissions.Speak & SpeakPermissions.Ls6) == SpeakPermissions.Ls6,
                [6] = (permissions.Speak & SpeakPermissions.Ls7) == SpeakPermissions.Ls7,
                [7] = (permissions.Speak & SpeakPermissions.Ls8) == SpeakPermissions.Ls8
            },
            
            // Cross-World Linkshells
            CrossWorldLinkshellValues =
            {
                [0] = (permissions.Speak & SpeakPermissions.Cwl1) == SpeakPermissions.Cwl1,
                [1] = (permissions.Speak & SpeakPermissions.Cwl2) == SpeakPermissions.Cwl2,
                [2] = (permissions.Speak & SpeakPermissions.Cwl3) == SpeakPermissions.Cwl3,
                [3] = (permissions.Speak & SpeakPermissions.Cwl4) == SpeakPermissions.Cwl4,
                [4] = (permissions.Speak & SpeakPermissions.Cwl5) == SpeakPermissions.Cwl5,
                [5] = (permissions.Speak & SpeakPermissions.Cwl6) == SpeakPermissions.Cwl6,
                [6] = (permissions.Speak & SpeakPermissions.Cwl7) == SpeakPermissions.Cwl7,
                [7] = (permissions.Speak & SpeakPermissions.Cwl8) == SpeakPermissions.Cwl8
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
        var primary = PrimaryPermissions.None;
        if (permissions.BodySwapValue) primary |= PrimaryPermissions.BodySwap;
        if (permissions.CustomizePlusValue) primary |= PrimaryPermissions.CustomizePlus;
        if (permissions.EmoteValue) primary |= PrimaryPermissions.Emote;
        if (permissions.GlamourerCustomizationsValue) primary |= PrimaryPermissions.GlamourerCustomization;
        if (permissions.GlamourerEquipmentValue) primary |= PrimaryPermissions.GlamourerEquipment;
        if (permissions.HonorificValue) primary |= PrimaryPermissions.Honorific;
        if (permissions.HypnosisValue) primary |= PrimaryPermissions.Hypnosis;
        if (permissions.MoodlesValue) primary |= PrimaryPermissions.Moodles;
        if (permissions.PenumbraModsValue) primary |= PrimaryPermissions.Mods;
        if (permissions.TwinningValue) primary |= PrimaryPermissions.Twinning;

        // Speak Permissions
        var speak = SpeakPermissions.None;
        if (permissions.AllianceValue) speak |= SpeakPermissions.Alliance;
        if (permissions.EchoValue) speak |= SpeakPermissions.Echo;
        if (permissions.FreeCompanyValue) speak |= SpeakPermissions.FreeCompany;
        if (permissions.PartyValue) speak |= SpeakPermissions.Party;
        if (permissions.PvPTeamValue) speak |= SpeakPermissions.PvPTeam;
        if (permissions.RoleplayValue) speak |= SpeakPermissions.Roleplay;
        if (permissions.SayValue) speak |= SpeakPermissions.Say;
        if (permissions.ShoutValue) speak |= SpeakPermissions.Shout;
        if (permissions.TellValue) speak |= SpeakPermissions.Tell;
        if (permissions.YellValue) speak |= SpeakPermissions.Yell;

        // Linkshells
        if (permissions.LinkshellValues[0]) speak |= SpeakPermissions.Ls1;
        if (permissions.LinkshellValues[1]) speak |= SpeakPermissions.Ls2;
        if (permissions.LinkshellValues[2]) speak |= SpeakPermissions.Ls3;
        if (permissions.LinkshellValues[3]) speak |= SpeakPermissions.Ls4;
        if (permissions.LinkshellValues[4]) speak |= SpeakPermissions.Ls5;
        if (permissions.LinkshellValues[5]) speak |= SpeakPermissions.Ls6;
        if (permissions.LinkshellValues[6]) speak |= SpeakPermissions.Ls7;
        if (permissions.LinkshellValues[7]) speak |= SpeakPermissions.Ls8;

        // Cross-world Linkshells
        if (permissions.CrossWorldLinkshellValues[0]) speak |= SpeakPermissions.Cwl1;
        if (permissions.CrossWorldLinkshellValues[1]) speak |= SpeakPermissions.Cwl2;
        if (permissions.CrossWorldLinkshellValues[2]) speak |= SpeakPermissions.Cwl3;
        if (permissions.CrossWorldLinkshellValues[3]) speak |= SpeakPermissions.Cwl4;
        if (permissions.CrossWorldLinkshellValues[4]) speak |= SpeakPermissions.Cwl5;
        if (permissions.CrossWorldLinkshellValues[5]) speak |= SpeakPermissions.Cwl6;
        if (permissions.CrossWorldLinkshellValues[6]) speak |= SpeakPermissions.Cwl7;
        if (permissions.CrossWorldLinkshellValues[7]) speak |= SpeakPermissions.Cwl8;

        // Elevated Permissions
        var elevated = ElevatedPermissions.None;
        if (permissions.PermanentTransformationValue) elevated |= ElevatedPermissions.PermanentTransformation;
        if (permissions.PossessionValue) elevated |= ElevatedPermissions.Possession;

        return new ResolvedPermissions(primary, speak, elevated);
    }
}