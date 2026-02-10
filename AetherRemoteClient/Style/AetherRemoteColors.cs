using System.Numerics;
using Dalamud.Bindings.ImGui;

namespace AetherRemoteClient.Style;

public static class AetherRemoteColors
{
    public static readonly uint PrimaryColor = ImGui.ColorConvertFloat4ToU32(new Vector4(0.9372f, 0.2862f, 0.3451f, 0.75f));
    public static readonly uint PrimaryColorAccent = ImGui.ColorConvertFloat4ToU32(new Vector4(0.9372f, 0.2862f, 0.3451f, 0.9f));
    public static readonly uint PanelColor = ImGui.ColorConvertFloat4ToU32(new Vector4(0.1294f, 0.1333f, 0.1764f, 1));
    public static readonly uint BackgroundColor = ImGui.ColorConvertFloat4ToU32(new Vector4(0.0431f, 0.0549f, 0.0588f, 0.95f));
    public static readonly uint DiscordBlue = ImGui.ColorConvertFloat4ToU32(new Vector4(0.4666f, 0.5215f, 0.8f, 1));
    
    public const ushort TextColorGreen = 45;
    public const ushort TextColorPurple = 48;
}
