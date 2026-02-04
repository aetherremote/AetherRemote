using System;
using AetherRemoteClient.Domain.Enums;
using AetherRemoteClient.UI.Views.Friends.Ui;
using AetherRemoteCommon.Domain;
using AetherRemoteCommon.Domain.Enums.Permissions;

namespace AetherRemoteClient.Domain;

/// <summary>
///     A permission set for individual permissions for use in <see cref="FriendsViewUi"/>
/// </summary>
public class IndividualPermissions
{
    // Note
    public string Note = string.Empty;
    
    // Primary Permissions
    public PermissionValue BodySwapValue = PermissionValue.Inherit;
    public PermissionValue CustomizePlusValue = PermissionValue.Inherit;
    public PermissionValue EmoteValue = PermissionValue.Inherit;
    public PermissionValue GlamourerCustomizationsValue = PermissionValue.Inherit;
    public PermissionValue GlamourerEquipmentValue = PermissionValue.Inherit;
    public PermissionValue HonorificValue = PermissionValue.Inherit;
    public PermissionValue HypnosisValue = PermissionValue.Inherit;
    public PermissionValue MoodlesValue = PermissionValue.Inherit;
    public PermissionValue PenumbraModsValue = PermissionValue.Inherit;
    public PermissionValue TwinningValue = PermissionValue.Inherit;
    
    // Speak Permissions
    public PermissionValue AllianceValue = PermissionValue.Inherit;
    public PermissionValue EchoValue = PermissionValue.Inherit;
    public PermissionValue FreeCompanyValue = PermissionValue.Inherit;
    public PermissionValue PartyValue = PermissionValue.Inherit;
    public PermissionValue PvPTeamValue = PermissionValue.Inherit;
    public PermissionValue RoleplayValue = PermissionValue.Inherit;
    public PermissionValue SayValue = PermissionValue.Inherit;
    public PermissionValue ShoutValue = PermissionValue.Inherit;
    public PermissionValue TellValue = PermissionValue.Inherit;
    public PermissionValue YellValue = PermissionValue.Inherit;
    
    public readonly PermissionValue[] LinkshellValues =
    [
        PermissionValue.Inherit,
        PermissionValue.Inherit,
        PermissionValue.Inherit,
        PermissionValue.Inherit,
        PermissionValue.Inherit,
        PermissionValue.Inherit,
        PermissionValue.Inherit,
        PermissionValue.Inherit
    ];

    public readonly PermissionValue[] CrossWorldLinkshellValues =
    [
        PermissionValue.Inherit,
        PermissionValue.Inherit,
        PermissionValue.Inherit,
        PermissionValue.Inherit,
        PermissionValue.Inherit,
        PermissionValue.Inherit,
        PermissionValue.Inherit,
        PermissionValue.Inherit
    ];
    
    // Elevated Permissions
    public PermissionValue PermanentTransformationValue = PermissionValue.Inherit;
    public PermissionValue PossessionValue = PermissionValue.Inherit;

    /// <summary>
    ///     Tests if one Individual Permissions value is equal to another
    /// </summary>
    public bool IsEqualTo(IndividualPermissions? other)
    {
        // Just check null in case
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
    ///     Converts a set of <see cref="RawPermissions"/> into <see cref="IndividualPermissions"/>
    /// </summary>
    public static IndividualPermissions From(RawPermissions permissions)
    {
        return new IndividualPermissions
        {
            // Primary Permissions
            BodySwapValue = ResolveFlag(PrimaryPermissions2.BodySwap, permissions.PrimaryAllow, permissions.PrimaryDeny),
            CustomizePlusValue = ResolveFlag(PrimaryPermissions2.CustomizePlus, permissions.PrimaryAllow, permissions.PrimaryDeny),
            EmoteValue = ResolveFlag(PrimaryPermissions2.Emote, permissions.PrimaryAllow, permissions.PrimaryDeny),
            GlamourerCustomizationsValue = ResolveFlag(PrimaryPermissions2.GlamourerCustomization, permissions.PrimaryAllow, permissions.PrimaryDeny),
            GlamourerEquipmentValue = ResolveFlag(PrimaryPermissions2.GlamourerEquipment, permissions.PrimaryAllow, permissions.PrimaryDeny),
            HonorificValue = ResolveFlag(PrimaryPermissions2.Honorific, permissions.PrimaryAllow, permissions.PrimaryDeny),
            HypnosisValue = ResolveFlag(PrimaryPermissions2.Hypnosis, permissions.PrimaryAllow, permissions.PrimaryDeny),
            MoodlesValue = ResolveFlag(PrimaryPermissions2.Moodles, permissions.PrimaryAllow, permissions.PrimaryDeny),
            PenumbraModsValue = ResolveFlag(PrimaryPermissions2.Mods, permissions.PrimaryAllow, permissions.PrimaryDeny),
            TwinningValue = ResolveFlag(PrimaryPermissions2.Twinning, permissions.PrimaryAllow, permissions.PrimaryDeny),

            // Speak Permissions
            AllianceValue = ResolveFlag(SpeakPermissions2.Alliance, permissions.SpeakAllow, permissions.SpeakDeny),
            EchoValue = ResolveFlag(SpeakPermissions2.Echo, permissions.SpeakAllow, permissions.SpeakDeny),
            FreeCompanyValue = ResolveFlag(SpeakPermissions2.FreeCompany, permissions.SpeakAllow, permissions.SpeakDeny),
            PartyValue = ResolveFlag(SpeakPermissions2.Party, permissions.SpeakAllow, permissions.SpeakDeny),
            PvPTeamValue = ResolveFlag(SpeakPermissions2.PvPTeam, permissions.SpeakAllow, permissions.SpeakDeny),
            RoleplayValue = ResolveFlag(SpeakPermissions2.Roleplay, permissions.SpeakAllow, permissions.SpeakDeny),
            SayValue = ResolveFlag(SpeakPermissions2.Say, permissions.SpeakAllow, permissions.SpeakDeny),
            ShoutValue = ResolveFlag(SpeakPermissions2.Shout, permissions.SpeakAllow, permissions.SpeakDeny),
            TellValue = ResolveFlag(SpeakPermissions2.Tell, permissions.SpeakAllow, permissions.SpeakDeny),
            YellValue = ResolveFlag(SpeakPermissions2.Yell, permissions.SpeakAllow, permissions.SpeakDeny),

            // Linkshell
            LinkshellValues =
            {
                [0] = ResolveFlag(SpeakPermissions2.Ls1, permissions.SpeakAllow, permissions.SpeakDeny),
                [1] = ResolveFlag(SpeakPermissions2.Ls2, permissions.SpeakAllow, permissions.SpeakDeny),
                [2] = ResolveFlag(SpeakPermissions2.Ls3, permissions.SpeakAllow, permissions.SpeakDeny),
                [3] = ResolveFlag(SpeakPermissions2.Ls4, permissions.SpeakAllow, permissions.SpeakDeny),
                [4] = ResolveFlag(SpeakPermissions2.Ls5, permissions.SpeakAllow, permissions.SpeakDeny),
                [5] = ResolveFlag(SpeakPermissions2.Ls6, permissions.SpeakAllow, permissions.SpeakDeny),
                [6] = ResolveFlag(SpeakPermissions2.Ls7, permissions.SpeakAllow, permissions.SpeakDeny),
                [7] = ResolveFlag(SpeakPermissions2.Ls8, permissions.SpeakAllow, permissions.SpeakDeny)
            },

            // Cross-world Linkshell
            CrossWorldLinkshellValues =
            {
                [0] = ResolveFlag(SpeakPermissions2.Cwl1, permissions.SpeakAllow, permissions.SpeakDeny),
                [1] = ResolveFlag(SpeakPermissions2.Cwl2, permissions.SpeakAllow, permissions.SpeakDeny),
                [2] = ResolveFlag(SpeakPermissions2.Cwl3, permissions.SpeakAllow, permissions.SpeakDeny),
                [3] = ResolveFlag(SpeakPermissions2.Cwl4, permissions.SpeakAllow, permissions.SpeakDeny),
                [4] = ResolveFlag(SpeakPermissions2.Cwl5, permissions.SpeakAllow, permissions.SpeakDeny),
                [5] = ResolveFlag(SpeakPermissions2.Cwl6, permissions.SpeakAllow, permissions.SpeakDeny),
                [6] = ResolveFlag(SpeakPermissions2.Cwl7, permissions.SpeakAllow, permissions.SpeakDeny),
                [7] = ResolveFlag(SpeakPermissions2.Cwl8, permissions.SpeakAllow, permissions.SpeakDeny)
            },

            // Elevated
            PermanentTransformationValue = ResolveFlag(ElevatedPermissions.PermanentTransformation, permissions.ElevatedAllow, permissions.ElevatedDeny),
            PossessionValue = ResolveFlag(ElevatedPermissions.Possession, permissions.ElevatedAllow, permissions.ElevatedDeny)
        };
    }

    /// <summary>
    ///     Converts a set of <see cref="IndividualPermissions"/> into <see cref="RawPermissions"/>
    /// </summary>
    public static RawPermissions To(IndividualPermissions permissions)
    {
        // Masks
        var primaryAllowMask = PrimaryPermissions2.None;
        var primaryDenyMask = PrimaryPermissions2.None;
        var speakAllowMask = SpeakPermissions2.None;
        var speakDenyMask = SpeakPermissions2.None;
        var elevatedAllowMask = ElevatedPermissions.None;
        var elevatedDenyMask = ElevatedPermissions.None;
        
        // Primary Permissions
        Apply(PrimaryPermissions2.BodySwap, permissions.BodySwapValue, ref primaryAllowMask, ref primaryDenyMask);
        Apply(PrimaryPermissions2.CustomizePlus, permissions.CustomizePlusValue, ref primaryAllowMask, ref primaryDenyMask);
        Apply(PrimaryPermissions2.Emote, permissions.EmoteValue, ref primaryAllowMask, ref primaryDenyMask);
        Apply(PrimaryPermissions2.GlamourerCustomization, permissions.GlamourerCustomizationsValue, ref primaryAllowMask, ref primaryDenyMask);
        Apply(PrimaryPermissions2.GlamourerEquipment, permissions.GlamourerEquipmentValue, ref primaryAllowMask, ref primaryDenyMask);
        Apply(PrimaryPermissions2.Honorific, permissions.HonorificValue, ref primaryAllowMask, ref primaryDenyMask);
        Apply(PrimaryPermissions2.Hypnosis, permissions.HypnosisValue, ref primaryAllowMask, ref primaryDenyMask);
        Apply(PrimaryPermissions2.Moodles, permissions.MoodlesValue, ref primaryAllowMask, ref primaryDenyMask);
        Apply(PrimaryPermissions2.Mods, permissions.PenumbraModsValue, ref primaryAllowMask, ref primaryDenyMask);
        Apply(PrimaryPermissions2.Twinning, permissions.TwinningValue, ref primaryAllowMask, ref primaryDenyMask);
        
        // Speak Permissions
        Apply(SpeakPermissions2.Alliance, permissions.AllianceValue, ref speakAllowMask, ref speakDenyMask);
        Apply(SpeakPermissions2.Echo, permissions.EchoValue, ref speakAllowMask, ref speakDenyMask);
        Apply(SpeakPermissions2.FreeCompany, permissions.FreeCompanyValue, ref speakAllowMask, ref speakDenyMask);
        Apply(SpeakPermissions2.Party, permissions.PartyValue, ref speakAllowMask, ref speakDenyMask);
        Apply(SpeakPermissions2.PvPTeam, permissions.PvPTeamValue, ref speakAllowMask, ref speakDenyMask);
        Apply(SpeakPermissions2.Roleplay, permissions.RoleplayValue, ref speakAllowMask, ref speakDenyMask);
        Apply(SpeakPermissions2.Say, permissions.SayValue, ref speakAllowMask, ref speakDenyMask);
        Apply(SpeakPermissions2.Shout, permissions.ShoutValue, ref speakAllowMask, ref speakDenyMask);
        Apply(SpeakPermissions2.Tell, permissions.TellValue, ref speakAllowMask, ref speakDenyMask);
        Apply(SpeakPermissions2.Yell, permissions.YellValue, ref speakAllowMask, ref speakDenyMask);
        
        // Linkshell
        Apply(SpeakPermissions2.Ls1, permissions.LinkshellValues[0], ref speakAllowMask, ref speakDenyMask);
        Apply(SpeakPermissions2.Ls2, permissions.LinkshellValues[1], ref speakAllowMask, ref speakDenyMask);
        Apply(SpeakPermissions2.Ls3, permissions.LinkshellValues[2], ref speakAllowMask, ref speakDenyMask);
        Apply(SpeakPermissions2.Ls4, permissions.LinkshellValues[3], ref speakAllowMask, ref speakDenyMask);
        Apply(SpeakPermissions2.Ls5, permissions.LinkshellValues[4], ref speakAllowMask, ref speakDenyMask);
        Apply(SpeakPermissions2.Ls6, permissions.LinkshellValues[5], ref speakAllowMask, ref speakDenyMask);
        Apply(SpeakPermissions2.Ls7, permissions.LinkshellValues[6], ref speakAllowMask, ref speakDenyMask);
        Apply(SpeakPermissions2.Ls8, permissions.LinkshellValues[7], ref speakAllowMask, ref speakDenyMask);
        
        // Cross-world Linkshell
        Apply(SpeakPermissions2.Cwl1, permissions.CrossWorldLinkshellValues[0], ref speakAllowMask, ref speakDenyMask);
        Apply(SpeakPermissions2.Cwl2, permissions.CrossWorldLinkshellValues[1], ref speakAllowMask, ref speakDenyMask);
        Apply(SpeakPermissions2.Cwl3, permissions.CrossWorldLinkshellValues[2], ref speakAllowMask, ref speakDenyMask);
        Apply(SpeakPermissions2.Cwl4, permissions.CrossWorldLinkshellValues[3], ref speakAllowMask, ref speakDenyMask);
        Apply(SpeakPermissions2.Cwl5, permissions.CrossWorldLinkshellValues[4], ref speakAllowMask, ref speakDenyMask);
        Apply(SpeakPermissions2.Cwl6, permissions.CrossWorldLinkshellValues[5], ref speakAllowMask, ref speakDenyMask);
        Apply(SpeakPermissions2.Cwl7, permissions.CrossWorldLinkshellValues[6], ref speakAllowMask, ref speakDenyMask);
        Apply(SpeakPermissions2.Cwl8, permissions.CrossWorldLinkshellValues[7], ref speakAllowMask, ref speakDenyMask);
        
        // Elevated
        Apply(ElevatedPermissions.PermanentTransformation, permissions.PermanentTransformationValue, ref elevatedAllowMask, ref elevatedDenyMask);
        Apply(ElevatedPermissions.Possession, permissions.PossessionValue, ref elevatedAllowMask, ref elevatedDenyMask);
        
        // Combine and return
        return new RawPermissions(primaryAllowMask, primaryDenyMask, speakAllowMask, speakDenyMask, elevatedAllowMask, elevatedDenyMask);
    }
    
    // We will utilize unmanaged and unsafe conversions because we know T will always have a [Flags] attribute
    private static void Apply<T>(T flag, PermissionValue value, ref T allowMask, ref T denyMask) where T : unmanaged, Enum
    {
        // Only safe because T has [Flags]
        var f = Convert.ToUInt64(flag);

        // Sort into correct mask
        switch (value)
        {
            case PermissionValue.Allow:
                allowMask = (T)Enum.ToObject(typeof(T), Convert.ToUInt64(allowMask) | f);
                break;
            
            case PermissionValue.Deny:
                denyMask = (T)Enum.ToObject(typeof(T), Convert.ToUInt64(denyMask) | f);
                break;
            
            case PermissionValue.Inherit:
            default:
                break;
        }
    }

    private static PermissionValue ResolveFlag<T>(T flag, T allow, T deny) where T : Enum
    {
        // Since we are working in generic T, we cannot use bitwise since we cannot ensure T has the [Flags] attribute
        var f = Convert.ToUInt64(flag);
        
        if ((Convert.ToUInt64(deny) & f) != 0)
            return PermissionValue.Deny;

        if ((Convert.ToUInt64(allow) & f) != 0)
            return PermissionValue.Allow;

        return PermissionValue.Inherit;
    }
}