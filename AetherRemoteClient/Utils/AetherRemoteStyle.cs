using System.Numerics;
using Dalamud.Bindings.ImGui;

namespace AetherRemoteClient.Utils;

public static class AetherRemoteStyle
{
    public const int Rounding = 8;
    public const ImGuiWindowFlags ContentFlags = ImGuiWindowFlags.NoBackground | ImGuiWindowFlags.NoScrollbar;
    public static readonly Vector2 ContentSize = new(-180, 0);
    public static readonly Vector2 NavBarDimensions = new(180, 0);
    public static readonly Vector4 PrimaryColor = new(0.9372f, 0.2862f, 0.3451f, 0.75f);
    public static readonly uint PanelBackground = ImGui.ColorConvertFloat4ToU32(new Vector4(0.1294f, 0.1333f, 0.1764f, 1));
    public static readonly uint ElevatedBackground = ImGui.ColorConvertFloat4ToU32(PrimaryColor);
    public static readonly uint DiscordBlue = ImGui.ColorConvertFloat4ToU32(new Vector4(0.4666f, 0.5215f, 0.8f, 1));

    public const ushort TextColorGreen = 45;
    public const ushort TextColorPurple = 48;
}