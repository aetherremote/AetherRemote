using System.Text;
using AetherRemoteCommon.Domain.Permissions;

namespace AetherRemoteCommon.Domain;

public class AetherRemoteStringBuilder
{
    private readonly StringBuilder _sb;
    private int _count;
    
    /// <summary>
    /// Wrapper class designed to ease formatting for objects in ToString methods
    /// </summary>
    public AetherRemoteStringBuilder(string name)
    {
        _sb = new StringBuilder();
        _sb.Append(name);
        _sb.Append('[');
    }

    /// <summary>
    /// Extension for <see cref="string"/>
    /// </summary>
    public void AddVariable(string name, string? value)
    {
        _sb.Append(name);
        _sb.Append('=');
        _sb.Append(value ?? "\"\"");
        _sb.Append(',');
        _count++;
    }
    
    /// <summary>
    /// Extension for <see cref="bool"/>
    /// </summary>
    public void AddVariable(string name, bool? value) => AddVariable(name, value.ToString());

    /// <summary>
    /// Extension for <see cref="List{String}"/>
    /// </summary>
    public void AddVariable(string name, List<string> values) => AddVariable(name, string.Join(", ", values));

    /// <summary>
    /// Extension for <see cref="HashSet{String}"/>
    /// </summary>
    public void AddVariable(string name, HashSet<string>? values) => AddVariable(name, string.Join(", ", values ?? []));

    /// <summary>
    /// Extension for <see cref="Enum"/>
    /// </summary>
    public void AddVariable<T>(string name, T value) where T : Enum => AddVariable(name, value.ToString());
    
    /// <summary>
    /// Extension for <see cref="UserPermissions"/>
    /// </summary>
    public void AddVariable(string name, UserPermissions value) => AddVariable(name, value.ToString());

    /// <summary>
    /// Extension for <see cref="UserPermissions"/>
    /// </summary>
    public void AddVariable(string name, Dictionary<string, UserPermissions>? value)
    {
        if (value is null)
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
        if (_count > 0)
            _sb.Length--;
        
        _sb.Append(']');
        return _sb.ToString();
    }
}
