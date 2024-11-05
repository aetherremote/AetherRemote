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
    private readonly Vector2 _dataPreviewTooltipSize;
    private readonly Vector2 _seeDataSelectableSize;

    private readonly string _data;
    private readonly string _id;

    /// <summary>
    /// <inheritdoc cref="HistoryLogWithGlamourer"/>
    /// </summary>
    public HistoryLogWithGlamourer(string message, string data) : base(message)
    {
        _data = data;
        _id = $"[Copy Data]###{Guid.NewGuid()}";
        _seeDataSelectableSize = new Vector2(ImGui.CalcTextSize("[Copy Data]").X, 0);
        _dataPreviewTooltipSize = new Vector2(300, 300);
    }

    /// <summary>
    /// <inheritdoc cref="AbstractHistoryLog.Build"/>
    /// </summary>
    public override void Build()
    {
        base.Build();
        ImGui.SameLine();

        ImGui.PushStyleColor(ImGuiCol.Text, ImGuiColors.ParsedOrange);
        if (ImGui.Selectable(_id, false, ImGuiSelectableFlags.None, _seeDataSelectableSize))
            ImGui.SetClipboardText(_data);
        ImGui.PopStyleColor();

        if (ImGui.IsItemHovered() == false) return;
        ImGui.SetNextWindowSize(_dataPreviewTooltipSize);
        ImGui.BeginTooltip();
        ImGui.TextWrapped(_data);
        ImGui.EndTooltip();
    }
}
