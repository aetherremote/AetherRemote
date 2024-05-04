using AetherRemoteCommon.Domain;
using System;

namespace AetherRemoteClient.Domain.Saves;

[Serializable]
public class SecretSave
{
    public string Secret { get; set; } = string.Empty;

    public override string ToString()
    {
        var sb = new AetherRemoteStringBuilder("SecretSave");
        sb.AddVariable("Secret", Secret);
        return sb.ToString();
    }
}
