using System.Numerics;
using Dalamud.Bindings.ImGui;

// ReSharper disable MemberCanBePrivate.Global

namespace AetherRemoteClient.Style;

/// <summary>
///     Container for static values corresponding to ImGui values in an effort to reduce expensive calls
/// </summary>
public static class AetherRemoteImGui
{
    public const int ChildBorderSize = 1;
    public const int ChildRounding = 8;
    public static readonly Vector2 CellPadding = new(4, 2);
    public static readonly Vector2 FramePadding = new(4, 3);
    public static readonly Vector2 ItemSpacing = new(8, 4);
    public static readonly Vector2 ItemInnerSpacing = new(4, 4);
    public static readonly Vector2 WindowPadding = new(8, 8);

    public static void Push()
    {
        ImGui.PushStyleVar(ImGuiStyleVar.ChildBorderSize, ChildBorderSize);
        ImGui.PushStyleVar(ImGuiStyleVar.ChildRounding, ChildRounding);
        ImGui.PushStyleVar(ImGuiStyleVar.CellPadding, CellPadding);
        ImGui.PushStyleVar(ImGuiStyleVar.FramePadding, FramePadding);
        ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, ItemSpacing);
        ImGui.PushStyleVar(ImGuiStyleVar.ItemInnerSpacing, ItemInnerSpacing);
        ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, WindowPadding);
        ImGui.PushStyleColor(ImGuiCol.Border, AetherRemoteColors.PanelColor);
        ImGui.PushStyleColor(ImGuiCol.ButtonActive, AetherRemoteColors.PrimaryColor);
        ImGui.PushStyleColor(ImGuiCol.ButtonHovered, AetherRemoteColors.PrimaryColorAccent);
        ImGui.PushStyleColor(ImGuiCol.ChildBg, AetherRemoteColors.PanelColor);
        ImGui.PushStyleColor(ImGuiCol.WindowBg, AetherRemoteColors.BackgroundColor);
    }

    public static void Pop()
    {
        ImGui.PopStyleVar(7);
        ImGui.PopStyleColor(5);
    }
}
