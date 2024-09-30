using AetherRemoteCommon.Domain.CommonChatMode;
using AetherRemoteCommon.Domain.CommonGlamourerApplyType;
using System.Text;

namespace AetherRemoteCommon.Domain;

public class AetherRemoteStringBuilder
{
    private readonly StringBuilder sb;
    private int count = 0;

    /// <summary>
    /// Wrapper class designed to ease formatting for objects in ToString methods
    /// </summary>
    public AetherRemoteStringBuilder(string name)
    {
        sb = new StringBuilder();
        sb.Append(name);
        sb.Append('[');
    }

    public void AddVariable(string name, string? value)
    {
        sb.Append(name);
        sb.Append('=');
        sb.Append(value ?? "\"\"");
        sb.Append(',');
        count++;
    }

    /// <summary>
    /// Extension for <see cref="bool"/>
    /// </summary>
    public void AddVariable(string name, bool value)
    {
        AddVariable(name, value.ToString());
    }

    /// <summary>
    /// Extension for <see cref="bool?"/>
    /// </summary>
    public void AddVariable(string name, bool? value)
    {
        AddVariable(name, value.ToString());
    }

    /// <summary>
    /// Extension for <see cref="List{String}"/>
    /// </summary>
    public void AddVariable(string name, List<string> values)
    {
        AddVariable(name, string.Join(", ", values));
    }

    /// <summary>
    /// Extension for <see cref="HashSet{String}"/>
    /// </summary>
    public void AddVariable(string name, HashSet<string>? values)
    {
        AddVariable(name, string.Join(", ", values ?? []));
    }

    /// <summary>
    /// Extension for <see cref="GlamourerApplyFlag"/>
    /// </summary>
    public void AddVariable(string name, GlamourerApplyFlag value)
    {
        AddVariable(name, value.ToString());
    }

    /// <summary>
    /// Extension for <see cref="ChatMode"/>
    /// </summary>
    public void AddVariable(string name, ChatMode value)
    {
        AddVariable(name, value.ToString());
    }

    /// <summary>
    /// Extension for <see cref="UserPermissions"/>
    /// </summary>
    public void AddVariable(string name, UserPermissions value)
    {
        AddVariable(name, value.ToString());
    }

    /// <summary>
    /// Extension for <see cref="UserPermissions"/>
    /// </summary>
    public void AddVariable(string name, Dictionary<string, UserPermissions>? value)
    {
        if (value == null)
        {
            AddVariable(name, "[]");
            return;
        }

        var sb = new StringBuilder();
        sb.Append('[');
        foreach (var kvp in value)
        {
            sb.Append('{');
            sb.Append(kvp.Key);
            sb.Append(", [");
            sb.Append(kvp.Value);
            sb.Append("]}");
            sb.Append(", ");
        }

        if (sb.Length > 1)
            sb.Remove(sb.Length - 2, 2);

        sb.Append(']');

        AddVariable(name, sb.ToString());
    }

    public override string ToString()
    {
        if (count > 0)
            sb.Length--;
        sb.Append(']');
        return sb.ToString();
    }
}
