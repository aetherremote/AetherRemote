using Dalamud.Interface;
using Dalamud.Interface.Colors;
using Dalamud.Interface.ManagedFontAtlas;
using ImGuiNET;
using System;
using System.Numerics;
using System.Threading.Tasks;

namespace AetherRemoteClient.Domain.UI;

/// <summary>
/// Exposes multiple static methods that simplify the process of many ImGui objects
/// </summary>
public class SharedUserInterfaces
{
    public static readonly Vector4 HoveredColorTheme = ImGuiColors.ParsedOrange - new Vector4(0.2f, 0.2f, 0.2f, 0);
    public static readonly Vector4 SelectedColorTheme = ImGuiColors.ParsedOrange;

    public static readonly ImGuiWindowFlags PopupWindowFlags =
        ImGuiWindowFlags.NoTitleBar |
        ImGuiWindowFlags.NoMove |
        ImGuiWindowFlags.NoResize;

    private static readonly ImGuiWindowFlags ComboWithFilterFlags = PopupWindowFlags | ImGuiWindowFlags.ChildWindow;

    private static IFontHandle? BigFont;
    private static bool BigFontBuilt = false;
    private const int BigFontSize = 40;

    private static IFontHandle? MediumFont;
    private static bool MediumFontBuilt = false;
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
        if (ImGui.IsItemHovered())
        {
            ImGui.BeginTooltip();
            ImGui.Text(tip);
            ImGui.EndTooltip();
        }
    }

    /// <summary>
    /// Draws a tool tip for the last hovered ImGui component
    /// </summary>
    /// <param name="tip"></param>
    public static void Tooltip(string[] tips)
    {
        if (ImGui.IsItemHovered())
        {
            ImGui.BeginTooltip();
            foreach (var tip in tips)
            {
                ImGui.Text(tip);
            }
            ImGui.EndTooltip();
        }
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
    {
        FontText(text, MediumFont, MediumFontBuilt, color);
    }

    /// <summary>
    /// Draws text using the default font, centered, with optional color.
    /// </summary>
    public static void TextCentered(string text, Vector4? color = null, Vector2? offset = null)
    {
        FontTextCentered(text, null, false, offset, color);
    }

    /// <summary>
    /// Draws text using the big font, centered, with optional color.
    /// </summary>
    public static void BigTextCentered(string text, Vector4? color = null, Vector2? offset = null)
    {
        FontTextCentered(text, BigFont, BigFontBuilt, offset, color);
    }

    public static void PushBigFont() { BigFont?.Push(); }
    public static void PopBigFont() { BigFont?.Pop(); }

    public static void ComboWithFilter(ref string choice, string hint, ListFilter<string> filterHelper,
        string? id = null, ImGuiWindowFlags? flags = null)
    {
        var comboFilterFlags = flags ?? ComboWithFilterFlags;
        var comboFilterId = id == null ? "##ComboFilter" : $"##{id}-ComboFilter";
        var popupName = id == null ? "##ComboFilterPopup" : $"##{id}-ComboFilterPopup";

        var _sizeX = 200;
        var _sizeY = (20 * Math.Min(filterHelper.List.Count, 10)) + ImGui.GetStyle().WindowPadding.Y;

        ImGui.SetNextItemWidth(_sizeX);
        if (ImGui.InputTextWithHint(comboFilterId, hint, ref choice, 100))
            filterHelper.UpdateSearchTerm(choice);

        var isInputTextActive = ImGui.IsItemActive();
        var isInputTextActivated = ImGui.IsItemActivated();
        if (isInputTextActivated && ImGui.IsPopupOpen(popupName) == false)
            ImGui.OpenPopup(popupName);

        var _x = ImGui.GetItemRectMin().X;
        var _y = ImGui.GetCursorPosY() + ImGui.GetWindowPos().Y;
        ImGui.SetNextWindowPos(new Vector2(_x, _y));
        ImGui.SetNextWindowSize(new Vector2(_sizeX, _sizeY));

        if (ImGui.BeginPopup(popupName, comboFilterFlags))
        {
            foreach (var option in filterHelper.List)
            {
                if (ImGui.Selectable(option))
                    choice = option;
            }

            if (isInputTextActive == false && ImGui.IsWindowFocused() == false)
                ImGui.CloseCurrentPopup();

            ImGui.EndPopup();
        }
    }

    /// <summary>
    /// Wraps a predicate around <see cref="ImGui.BeginDisabled"/> and <see cref="ImGui.EndDisabled"/>
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

    /// <summary>
    /// Creates a description on the object before this call outlining a brief explaination and requirements
    /// </summary>
    public static void CommandDescription(
        string description,
        string[]? requiredPlugins = null,
        string[]? requiredPermissions = null,
        string[]? optionalPermissions = null)
    {
        if (ImGui.IsItemHovered())
        {
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

    // Grab whatever the default dalamud font is, and make a medium version, and a big version of it
    private async Task BuildDefaultFontExtraSizes()
    {
        BigFont = Plugin.PluginInterface.UiBuilder.FontAtlas.NewDelegateFontHandle(toolkit =>
        {
            toolkit.OnPreBuild(preBuild =>
            {
                preBuild.AddDalamudDefaultFont(BigFontSize);
            });
        });

        MediumFont = Plugin.PluginInterface.UiBuilder.FontAtlas.NewDelegateFontHandle(toolkit =>
        {
            toolkit.OnPreBuild(preBuild =>
            {
                preBuild.AddDalamudDefaultFont(MediumFontSize);
            });
        });

        await BigFont.WaitAsync();
        await MediumFont.WaitAsync();
        await Plugin.PluginInterface.UiBuilder.FontAtlas.BuildFontsAsync();

        BigFontBuilt = true;
        MediumFontBuilt = true;
    }
}
