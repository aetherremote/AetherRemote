using Dalamud.Interface;
using Dalamud.Interface.Colors;
using Dalamud.Interface.ManagedFontAtlas;
using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Threading.Tasks;

namespace AetherRemoteClient.Domain.UI;

/// <summary>
/// Exposes multiple static methods that simplify the process of many ImGui objects
/// </summary>
public class SharedUserInterfaces
{
    private const ImGuiWindowFlags PopupWindowFlags = ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoResize;
    private const ImGuiWindowFlags ComboWithFilterFlags = PopupWindowFlags | ImGuiWindowFlags.ChildWindow;

    private static IFontHandle? _bigFont;
    private static bool _bigFontBuilt;
    private const int BigFontSize = 40;

    private static IFontHandle? _mediumFont;
    private static bool _mediumFontBuilt;
    private const int MediumFontSize = 24;

    /// <summary>
    /// <inheritdoc cref="SharedUserInterfaces"/>
    /// </summary>
    public SharedUserInterfaces()
    {
        Task.Run(BuildDefaultFontExtraSizes);
    }

    /// <summary>
    /// Draws a tool tip for the last hovered ImGui component
    /// </summary>
    /// <param name="tip"></param>
    public static void Tooltip(string tip)
    {
        if (ImGui.IsItemHovered() is false) return;
        ImGui.BeginTooltip();
        ImGui.Text(tip);
        ImGui.EndTooltip();
    }

    /// <summary>
    /// Draws a tool tip for the last hovered ImGui component
    /// </summary>
    /// <param name="tips"></param>
    public static void Tooltip(string[] tips)
    {
        if (ImGui.IsItemHovered() is false) return;
        ImGui.BeginTooltip();
        foreach (var tip in tips)
        {
            ImGui.Text(tip);
        }
        ImGui.EndTooltip();
    }

    /// <summary>
    /// Draws a <see cref="FontAwesomeIcon"/>
    /// </summary>
    public static void Icon(FontAwesomeIcon icon, Vector4? color = null)
    {
        if (color.HasValue) ImGui.PushStyleColor(ImGuiCol.Text, color.Value);
        ImGui.PushFont(UiBuilder.IconFont);
        ImGui.TextUnformatted(icon.ToIconString());
        ImGui.PopFont();
        if (color.HasValue) ImGui.PopStyleColor();
    }

    /// <summary>
    /// Creates a button with specified icon
    /// </summary>
    public static bool IconButton(FontAwesomeIcon icon, Vector2? size = null, string? id = null)
    {
        ImGui.PushFont(UiBuilder.IconFont);

        if (id != null)
            ImGui.PushID(id);

        var result = ImGui.Button(icon.ToIconString(), size ?? Vector2.Zero);

        if (id != null)
            ImGui.PopID();

        ImGui.PopFont();
        return result;
    }

    /// <summary>
    /// Draws text using the medium font with optional color.
    /// </summary>
    public static void MediumText(string text, Vector4? color = null) 
        => FontText(text, _mediumFont, _mediumFontBuilt, color);

    /// <summary>
    /// Draws text using the default font, centered, with optional color.
    /// </summary>
    public static void TextCentered(string text, Vector4? color = null, Vector2? offset = null) 
        => FontTextCentered(text, null, false, offset, color);

    /// <summary>
    /// Draws text using the big font, centered, with optional color.
    /// </summary>
    public static void BigTextCentered(string text, Vector4? color = null, Vector2? offset = null) 
        => FontTextCentered(text, _bigFont, _bigFontBuilt, offset, color);

    public static void PushBigFont() => _bigFont?.Push();
    public static void PopBigFont() => _bigFont?.Pop();

    public static void ComboWithFilter(
        string id,
        string hint,
        ref string choice,
        float width,
        ListFilter<string> filterHelper,
        ImGuiWindowFlags? flags = null)
    {
        var comboFilterFlags = flags ?? ComboWithFilterFlags;
        var comboFilterId = $"##{id}-ComboFilter";
        var popupName = $"##{id}-ComboFilterPopup";

        var sizeY = (20 * Math.Min(filterHelper.List.Count, 10)) + ImGui.GetStyle().WindowPadding.Y;

        ImGui.SetNextItemWidth(width);
        if (ImGui.InputTextWithHint(comboFilterId, hint, ref choice, 100))
            filterHelper.UpdateSearchTerm(choice);

        var itemWidth = ImGui.GetItemRectSize().X;

        var isInputTextActive = ImGui.IsItemActive();
        var isInputTextActivated = ImGui.IsItemActivated();
        if (isInputTextActivated && ImGui.IsPopupOpen(popupName) == false)
            ImGui.OpenPopup(popupName);

        var x = ImGui.GetItemRectMin().X;
        var y = ImGui.GetCursorPosY() + ImGui.GetWindowPos().Y;
        ImGui.SetNextWindowPos(new Vector2(x, y));
        ImGui.SetNextWindowSize(new Vector2(itemWidth, sizeY));

        if (ImGui.BeginPopup(popupName, comboFilterFlags) is false) return;
        foreach (var option in filterHelper.List)
        {
            if (ImGui.Selectable(option))
                choice = option;
        }

        if (isInputTextActive == false && ImGui.IsWindowFocused() is false)
            ImGui.CloseCurrentPopup();

        ImGui.EndPopup();
    }

    /// <summary>
    /// Wraps a predicate around <see cref="ImGui.BeginDisabled()"/> and <see cref="ImGui.EndDisabled"/>
    /// </summary>
    public static void DisableIf(bool condition, Action action)
    {
        if (condition)
            ImGui.BeginDisabled();

        action.Invoke();

        if (condition)
            ImGui.EndDisabled();
    }

    /// <summary>
    /// Creates a <see cref="FontAwesomeIcon.QuestionCircle"/> displaying a description
    /// </summary>
    public static void CommandDescriptionWithQuestionMark(
        string description,
        string[]? requiredPlugins = null,
        string[]? requiredPermissions = null,
        string[]? optionalPermissions = null)
    {
        // Align previous text
        ImGui.AlignTextToFramePadding();
        Icon(FontAwesomeIcon.QuestionCircle);

        CommandDescription(description, requiredPlugins, requiredPermissions, optionalPermissions);
    }

    public static void PermissionsWarning(List<string> friendsMissingPermissions)
    {
        ImGui.PushStyleColor(ImGuiCol.Text, ImGuiColors.DalamudYellow);
        Icon(FontAwesomeIcon.ExclamationTriangle);
        ImGui.PopStyleColor();

        if (ImGui.IsItemHovered() is false) return;
        ImGui.BeginTooltip();
        ImGui.Text("Inadequate permissions!");
        ImGui.Separator();
        foreach (var friend in friendsMissingPermissions)
        {
            Plugin.Configuration.Notes.TryGetValue(friend, out var note);
            ImGui.Text(note ?? friend);
        }
        ImGui.EndTooltip();
    }

    /// <summary>
    /// Creates a description on the object before this call outlining a brief explanation and requirements
    /// </summary>
    public static void CommandDescription(
        string description,
        string[]? requiredPlugins = null,
        string[]? requiredPermissions = null,
        string[]? optionalPermissions = null)
    {
        if (ImGui.IsItemHovered() is false) return;
        ImGui.BeginTooltip();

        ImGui.TextColored(ImGuiColors.ParsedOrange, "[Description]");
        ImGui.Text(description);

        ImGui.Separator();

        if (requiredPlugins is not null)
        {
            ImGui.TextColored(ImGuiColors.ParsedOrange, "[Required Plugins]");
            foreach(var plugin in requiredPlugins)
                ImGui.BulletText(plugin);
        }

        if (requiredPermissions is not null)
        {
            ImGui.TextColored(ImGuiColors.ParsedOrange, "[Required Permissions]");
            foreach (var permissions in requiredPermissions)
                ImGui.BulletText(permissions);
        }

        if (optionalPermissions is not null)
        {
            ImGui.TextColored(ImGuiColors.ParsedOrange, "[Optional Permissions]");
            foreach (var permissions in optionalPermissions)
                ImGui.BulletText(permissions);
        }

        ImGui.EndTooltip();
    }

    /// <summary>
    /// Draws text using a specific font with optional color.
    /// </summary>
    private static void FontText(string text, IFontHandle? font, bool fontBuilt, Vector4? color = null)
    {
        if (color.HasValue) ImGui.PushStyleColor(ImGuiCol.Text, color.Value);
        if (fontBuilt) font?.Push();
        ImGui.TextUnformatted(text);
        if (fontBuilt) font?.Pop();
        if (color.HasValue) ImGui.PopStyleColor();
    }

    /// <summary>
    /// Draws text using a specific font, centered, with optional color.
    /// </summary>
    private static void FontTextCentered(string text, IFontHandle? font, bool fontBuilt, Vector2? offset = null, Vector4? color = null)
    {
        if (fontBuilt) font?.Push();

        offset ??= Vector2.Zero;
        color ??= ImGuiColors.DalamudWhite;

        var width = ImGui.GetWindowWidth();
        var textWidth = ImGui.CalcTextSize(text).X;

        ImGui.SetCursorPosX(((width - textWidth) * 0.5f) - offset.Value.X);
        ImGui.SetCursorPosY(ImGui.GetCursorPosY() - offset.Value.Y);
        ImGui.TextColored(color.Value, text);

        if (fontBuilt) font?.Pop();
    }

    // Grab whatever the default Dalamud font is, and make a medium version, and a big version of it
    private static async Task BuildDefaultFontExtraSizes()
    {
        _bigFont = Plugin.PluginInterface.UiBuilder.FontAtlas.NewDelegateFontHandle(toolkit =>
        {
            toolkit.OnPreBuild(preBuild =>
            {
                preBuild.AddDalamudDefaultFont(BigFontSize);
            });
        });

        _mediumFont = Plugin.PluginInterface.UiBuilder.FontAtlas.NewDelegateFontHandle(toolkit =>
        {
            toolkit.OnPreBuild(preBuild =>
            {
                preBuild.AddDalamudDefaultFont(MediumFontSize);
            });
        });

        await _bigFont.WaitAsync();
        await _mediumFont.WaitAsync();
        await Plugin.PluginInterface.UiBuilder.FontAtlas.BuildFontsAsync();

        _bigFontBuilt = true;
        _mediumFontBuilt = true;
    }
}
