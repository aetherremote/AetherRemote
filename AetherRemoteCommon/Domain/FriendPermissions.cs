namespace AetherRemoteCommon.Domain.CommonFriendPermissions;

[Serializable]
public class FriendPermissions
{
    public bool Speak { get; set; } = false;
    public bool Emote { get; set; } = false;
    public bool ChangeAppearance { get; set; } = false;
    public bool ChangeEquipment { get; set; } = false;
    public bool Say { get; set; } = false;
    public bool Yell { get; set; } = false;
    public bool Shout { get; set; } = false;
    public bool Tell { get; set; } = false;
    public bool Party { get; set; } = false;
    public bool Alliance { get; set; } = false;
    public bool FreeCompany { get; set; } = false;
    public bool Linkshell { get; set; } = false;
    public bool CrossworldLinkshell { get; set; } = false;
    public bool PvPTeam { get; set; } = false;

    public void SetAll(bool allowEmote, bool allowSpeak, bool allowChangeAppearance, bool allowChangeEquipment, bool allowSay, bool allowYell, bool allowShout,
        bool allowTell, bool allowParty, bool allowAlliance, bool allowFreeCompany, bool allowLinkshell, bool allowCrossworldLinkshell, bool allowPvPTeam)
    {
        Speak = allowSpeak;
        Emote = allowEmote;
        ChangeAppearance = allowChangeAppearance;
        ChangeEquipment = allowChangeEquipment;
        Say = allowSay;
        Yell = allowYell;
        Shout = allowShout;
        Tell = allowTell;
        Party = allowParty;
        Alliance = allowAlliance;
        FreeCompany = allowFreeCompany;
        Linkshell = allowLinkshell;
        CrossworldLinkshell = allowCrossworldLinkshell;
        PvPTeam = allowPvPTeam;
    }

    public override string ToString()
    {
        var sb = new AetherRemoteStringBuilder("FriendPermissions");
        sb.AddVariable("Speak", Speak);
        sb.AddVariable("Say", Say);
        sb.AddVariable("Yell", Yell);
        sb.AddVariable("Shout", Shout);
        sb.AddVariable("Tell", Tell);
        sb.AddVariable("Party", Party);
        sb.AddVariable("Alliance", Alliance);
        sb.AddVariable("FreeCompany", FreeCompany);
        sb.AddVariable("Linkshell", Linkshell);
        sb.AddVariable("CrossworldLinkshell", CrossworldLinkshell);
        sb.AddVariable("PvPTeam", PvPTeam);
        sb.AddVariable("Emote", Emote);
        sb.AddVariable("ChangeAppearance", ChangeAppearance);
        sb.AddVariable("ChangeEquipment", ChangeEquipment);
        return sb.ToString();
    }
}
