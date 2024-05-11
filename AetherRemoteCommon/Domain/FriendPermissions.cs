namespace AetherRemoteCommon.Domain.CommonFriendPermissions;

[Serializable]
public class FriendPermissions
{
    public bool Speak = false;
    public bool Emote = false;
    public bool ChangeAppearance = false;
    public bool ChangeEquipment = false;
    public bool Say = false;
    public bool Yell = false;
    public bool Shout = false;
    public bool Tell = false;
    public bool Party = false;
    public bool Alliance = false;
    public bool FreeCompany = false;
    public bool Linkshell = false;
    public bool CrossworldLinkshell = false;
    public bool PvPTeam = false;

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
