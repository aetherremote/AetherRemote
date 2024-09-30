using Dalamud.Interface.Colors;
using ImGuiNET;
using System;
using System.Numerics;

namespace AetherRemoteClient.Domain.Log;

/// <summary>
/// Stores history data with glamourer data included
/// </summary>
public class HistoryLogWithGlamourer : HistoryLog
{
    private readonly Vector2 dataPreviewTooltipSize;
    private readonly Vector2 seeDataSelectableSize;

    public readonly string Data;

    private readonly string id;

    /// <summary>
    /// <inheritdoc cref="HistoryLogWithGlamourer"/>
    /// </summary>
    public HistoryLogWithGlamourer(string message, string data) : base(message)
    {
        Data = data;
        id = $"[Copy Data]###{Guid.NewGuid()}";
        seeDataSelectableSize = new(ImGui.CalcTextSize("[Copy Data]").X, 0);
        dataPreviewTooltipSize = new Vector2(300, 300);
    }

    /// <summary>
    /// <inheritdoc cref="AbstractHistoryLog.Build"/>
    /// </summary>
    public override void Build()
    {
        base.Build();
        ImGui.SameLine();

        ImGui.PushStyleColor(ImGuiCol.Text, ImGuiColors.ParsedOrange);
        if (ImGui.Selectable(id, false, ImGuiSelectableFlags.None, seeDataSelectableSize))
            ImGui.SetClipboardText(Data);
        ImGui.PopStyleColor();

        if (ImGui.IsItemHovered())
        {
            ImGui.SetNextWindowSize(dataPreviewTooltipSize);
            ImGui.BeginTooltip();
            ImGui.TextWrapped(Data);
            ImGui.EndTooltip();
        }
    }
}
