namespace AetherRemoteCommon.Domain.CommonChatMode;

public enum ChatMode
{
    Say,
    Yell,
    Shout,
    Tell,
    Party,
    Alliance,
    FreeCompany,
    Linkshell,
    CrossWorldLinkshell,
    NoviceNetwork,
    PvPTeam,
}

public static class ChatModeTranslator
{
    public static string Beautify(this ChatMode chatMode)
    {
        return chatMode switch
        {
            ChatMode.Say or
            ChatMode.Yell or
            ChatMode.Shout or
            ChatMode.Tell or
            ChatMode.Party or
            ChatMode.Alliance => chatMode.ToString(),
            ChatMode.Linkshell => "LS",
            ChatMode.FreeCompany => "Free Company",
            ChatMode.CrossWorldLinkshell => "CWLS",
            ChatMode.NoviceNetwork => "Novice Network",
            ChatMode.PvPTeam => "PvP Team",
            _ => chatMode.ToString()
        }; ;
    }

    public static string Command(this ChatMode chatMode)
    {
        return chatMode switch
        {
            ChatMode.Say => "s",
            ChatMode.Yell => "y",
            ChatMode.Shout => "sh",
            ChatMode.Tell => "t",
            ChatMode.Party => "p",
            ChatMode.Alliance => "a",
            ChatMode.FreeCompany => "fc",
            ChatMode.Linkshell => "l",
            ChatMode.CrossWorldLinkshell => "cwl",
            ChatMode.NoviceNetwork => "n",
            ChatMode.PvPTeam => "pt",
            _ => throw new NotImplementedException()
        };
    }
}
