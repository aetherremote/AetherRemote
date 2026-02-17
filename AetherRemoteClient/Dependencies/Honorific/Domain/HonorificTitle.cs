using System.Numerics;

namespace AetherRemoteClient.Dependencies.Honorific.Domain;

/// <summary>
///     The model representing Honorific's <see href="https://github.com/Caraxi/Honorific/blob/master/CustomTitle.cs">CustomTitle</see>class
/// </summary>
public class HonorificCustomTitle(string title, bool isPrefix, Vector3? color, Vector3? glow)
{
    public string Title = title;
    public bool IsPrefix = isPrefix;
    public Vector3? Color = color;
    public Vector3? Glow = glow;
}