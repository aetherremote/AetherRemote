using AetherRemoteClient.Domain.Translators;
using AetherRemoteCommon.Domain;
using System;
using System.Collections.Generic;

namespace AetherRemoteClient.Domain.Saves;

[Serializable]
public class FriendListSave
{
    public string Version { get; set; } = "1.0.0.0";
    public List<Friend> Friends { get; set; } = [];

    public override string ToString()
    {
        var sb = new AetherRemoteStringBuilder("FriendListSave");
        sb.AddVariable("Version", Version);
        sb.AddVariable("Friends", FriendTranslator.DomainFriendListToCommon(Friends));
        return sb.ToString();
    }
}
