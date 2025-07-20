using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Threading.Tasks;
using AetherRemoteClient.Domain;
using Dalamud.Interface;
using Dalamud.Interface.Colors;
using Dalamud.Interface.ManagedFontAtlas;
using ImGuiNET;

namespace AetherRemoteClient.Utils;

/// <summary>
///     Exposes multiple static methods that simplify the process of many ImGui objects
/// </summary>
public static class SharedUserInterfaces
{
    private const ImGuiWindowFlags PopupWindowFlags =
        ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoResize;

    private const ImGuiWindowFlags ComboWithFilterFlags = PopupWindowFlags | ImGuiWindowFlags.ChildWindow;
    
    public const int MassiveFontSize = 300;
    public static ImFontPtr MassiveFont { get; private set; }
    private static IFontHandle? _massiveFont;
    
    private static readonly SafeFontConfig DefaultFontConfig = new() { SizePx = MassiveFontSize };
    
    public const int BigFontSize = 40;
    private static IFontHandle? _bigFont;

    private const int MediumFontSize = 24;
    private static IFontHandle? _mediumFont;
    
    /// <summary>
    ///     Draws a tool tip for the last hovered ImGui component
    /// </summary>
    /// <param name="tip"></param>
    public static void Tooltip(string tip)
    {
        if (ImGui.IsItemHovered() is false)
            return;

        ImGui.SetTooltip(tip);
    }

    /// <summary>
    ///     Draws a tool tip for the last hovered ImGui component
    /// </summary>
    /// <param name="tips"></param>
    public static void Tooltip(string[] tips)
    {
        if (ImGui.IsItemHovered() is false)
            return;
        
        ImGui.SetTooltip(string.Join(Environment.NewLine, tips));
    }

    /// <summary>
    ///     Draws a <see cref="FontAwesomeIcon"/>
    /// </summary>
    public static void Icon(FontAwesomeIcon icon, Vector4? color = null)
    {
        ImGui.PushFont(UiBuilder.IconFont);
        if (color is null)
            ImGui.TextUnformatted(icon.ToIconString());
        else
            ImGui.TextColored(color.Value, icon.ToIconString());
        ImGui.PopFont();
    }

    /// <summary>
    ///     Creates a button with specified icon
    /// </summary>
    public static bool IconButton(FontAwesomeIcon icon, Vector2? size = null, string? tooltip = null, string? id = null)
    {
        var raw = id is null
            ? string.Concat(icon.ToIconString())
            : string.Concat(icon.ToIconString(), "##", id);
        
        ImGui.PushFont(UiBuilder.IconFont);
        var result = ImGui.Button(raw, size ?? Vector2.Zero);
        ImGui.PopFont();

        if (tooltip is null || ImGui.IsItemHovered() is false)
            return result;

        ImGui.SetTooltip(tooltip);
        return result;
    }

    /// <summary>
    ///     For use with creating buttons in a toggleable state
    /// </summary>
    /// <param name="icon"></param>
    /// <param name="size"></param>
    /// <param name="tooltip"></param>
    /// <param name="selected"></param>
    /// <returns></returns>
    public static bool IconOptionButton(FontAwesomeIcon icon, Vector2 size, string tooltip, bool selected)
    {
        ImGui.PushFont(UiBuilder.IconFont);
        
        bool result;
        if (selected)
        {
            ImGui.PushStyleColor(ImGuiCol.Button, AetherRemoteStyle.PrimaryColor);
            result = ImGui.Button(icon.ToIconString(), size);
            ImGui.PopStyleColor();
        }
        else
        {
            result = ImGui.Button(icon.ToIconString(), size);
        }
        
        ImGui.PopFont();
        
        if (ImGui.IsItemHovered())
            ImGui.SetTooltip(tooltip);
        
        return result;
    }

    /// <summary>
    ///     Draws text using the medium font with optional color.
    /// </summary>
    public static void MediumText(string text, Vector4? color = null)
    {
        _mediumFont?.Push();
        
        if (color is null)
            ImGui.TextUnformatted(text);
        else
            ImGui.TextColored(color.Value, text);

        _mediumFont?.Pop();
    }

    public static void MediumSelectableText(string text, string? tooltip = null, Vector4? color = null)
    {
        _mediumFont?.Push();

        if (color is null)
        {
            ImGui.Selectable(text, false, ImGuiSelectableFlags.None, ImGui.CalcTextSize(text));
        }
        else
        {
            ImGui.PushStyleColor(ImGuiCol.Text, ImGuiColors.ParsedGreen);
            ImGui.Selectable(text, false, ImGuiSelectableFlags.None, ImGui.CalcTextSize(text));
            ImGui.PopStyleColor();
        }
        
        _mediumFont?.Pop();
        
        if (tooltip is null)
            return;
        
        if (ImGui.IsItemHovered())
            ImGui.SetTooltip(tooltip);
    }

    /// <summary>
    ///     Draws text using the default font, centered, with optional color.
    /// </summary>
    public static void TextCentered(string text, Vector4? color = null)
    {
        ImGui.SetCursorPosX((ImGui.GetWindowWidth() - ImGui.CalcTextSize(text).X) * 0.5f);
        if (color is null)
            ImGui.TextUnformatted(text);
        else
            ImGui.TextColored(color.Value, text);
    }

    /// <summary>
    ///     Draws text using the big font, centered, with optional color.
    /// </summary>
    public static void BigTextCentered(string text, Vector4? color = null)
    {
        _bigFont?.Push();

        ImGui.SetCursorPosX((ImGui.GetWindowWidth() - ImGui.CalcTextSize(text).X) * 0.5f);
        if (color is null)
            ImGui.TextUnformatted(text);
        else
            ImGui.TextColored(color.Value, text);

        _bigFont?.Pop();
    }
    
    public static void PushMassiveFont() => _massiveFont?.Push();
    public static void PopMassiveFont() => _massiveFont?.Pop();
    public static void PushBigFont() => _bigFont?.Push();
    public static void PopBigFont() => _bigFont?.Pop();
    
    /// <summary>
    ///     Creates a button the size of a <see cref="ContentBox"/> on the right
    /// </summary>
    public static bool ContextBoxButton(FontAwesomeIcon icon, Vector2 padding, float windowWidth)
    {
        var previousRectSize = ImGui.GetItemRectSize();
        var returnPoint = ImGui.GetCursorPosY();
        var begin = returnPoint - previousRectSize.Y - padding.Y * 2;
        
        var x = windowWidth - previousRectSize.Y - padding.X;
        var size = new Vector2(x, begin);
        
        ImGui.SetCursorPos(size);
        var clicked = IconButton(icon, new Vector2(previousRectSize.Y));
        ImGui.SetCursorPosY(returnPoint);
        return clicked;
    }

    public static void ComboWithFilter(string id, string hint, ref string choice, float width,
        ListFilter<string> filterHelper, ImGuiWindowFlags? flags = null)
    {
        var comboFilterFlags = flags ?? ComboWithFilterFlags;
        var comboFilterId = $"##{id}-ComboFilter";
        var popupName = $"##{id}-ComboFilterPopup";

        var sizeY = 20 * Math.Min(filterHelper.List.Count, 10) + ImGui.GetStyle().WindowPadding.Y;

        ImGui.SetNextItemWidth(width);
        if (ImGui.InputTextWithHint(comboFilterId, hint, ref choice, 100))
            filterHelper.UpdateSearchTerm(choice);

        var itemWidth = ImGui.GetItemRectSize().X;
        var isInputTextActive = ImGui.IsItemActive();
        var isInputTextActivated = ImGui.IsItemActivated();
        var popupIsOpen = ImGui.IsPopupOpen(popupName);
        if (isInputTextActivated && popupIsOpen is false)
            ImGui.OpenPopup(popupName);

        if (popupIsOpen)
        {
            var x = ImGui.GetItemRectMin().X;
            var y = ImGui.GetCursorPosY() + ImGui.GetWindowPos().Y;
            ImGui.SetNextWindowPos(new Vector2(x, y));
            ImGui.SetNextWindowSize(new Vector2(itemWidth, sizeY));
        }

        if (ImGui.BeginPopup(popupName, comboFilterFlags) is false) return;
        foreach (var option in filterHelper.List)
            if (ImGui.Selectable(option))
                choice = option;

        if (isInputTextActive is false && ImGui.IsWindowFocused() is false)
            ImGui.CloseCurrentPopup();

        ImGui.EndPopup();
    }
    
    /// <summary>
    ///     Dictionary used to store the calculated sizes of all content boxes
    /// </summary>
    private static readonly Dictionary<string, Vector2> ContextBoxSizeCache = [];
    
    /// <summary>
    ///     Draws a box of arbitrary size
    /// </summary>
    /// <param name="id">Unique ID for caching purposes</param>
    /// <param name="backgroundColor">Color of the background box</param>
    /// <param name="includeEndPadding">Should padding be added at the end?</param>
    /// <param name="contentToDraw">What should be drawn in this box</param>
    public static void ContentBox(string id, uint backgroundColor, bool includeEndPadding, Action contentToDraw)
    {
        var draw = ImGui.GetWindowDrawList();
        var padding = ImGui.GetStyle().WindowPadding;
        var startCursorPos = ImGui.GetCursorPos();
        var startScreenPos = ImGui.GetCursorScreenPos();

        if (ContextBoxSizeCache.TryGetValue(id, out var cached))
            draw.AddRectFilled(startScreenPos, startScreenPos + cached, backgroundColor, AetherRemoteStyle.Rounding);
        
        ImGui.SetCursorPos(startCursorPos + padding);
        
        ImGui.BeginGroup();
        contentToDraw.Invoke();
        ImGui.EndGroup();

        var size = ImGui.GetItemRectSize() + padding * 2;
        size.X = ImGui.GetWindowWidth();
        
        if (cached.Equals(size) is false)
            ContextBoxSizeCache[id] = size;
        
        ImGui.SetCursorPosY(startCursorPos.Y + size.Y + (includeEndPadding ? padding.Y : 0));
    }

    /// <summary>
    /// Initializes the two additional font sizes used in the plugin
    /// </summary>
    public static async Task InitializeFonts()
    {
        string? dalamudFontDirectory = null;
        try
        {
            var path = Path.Combine(Plugin.PluginInterface.DalamudAssetDirectory.FullName, "UIRes",
                "Inconsolata-Regular.ttf");
            
            if (File.Exists(path))
                dalamudFontDirectory = path;
        }
        catch (Exception e)
        {
            Plugin.Log.Warning($"Unexpectedly failed to read `Inconsolata-Regular` font, {e.Message}");
        }
        
        _massiveFont = Plugin.PluginInterface.UiBuilder.FontAtlas.NewDelegateFontHandle(toolkit =>
        {
            toolkit.OnPreBuild(preBuild =>
            {
                MassiveFont = dalamudFontDirectory is null 
                    ? preBuild.AddDalamudDefaultFont(MassiveFontSize) 
                    : preBuild.AddFontFromFile(dalamudFontDirectory, DefaultFontConfig);
            });
        });
        
        _bigFont = Plugin.PluginInterface.UiBuilder.FontAtlas.NewDelegateFontHandle(toolkit =>
        {
            toolkit.OnPreBuild(preBuild => { preBuild.AddDalamudDefaultFont(BigFontSize); });
        });

        _mediumFont = Plugin.PluginInterface.UiBuilder.FontAtlas.NewDelegateFontHandle(toolkit =>
        {
            toolkit.OnPreBuild(preBuild => { preBuild.AddDalamudDefaultFont(MediumFontSize); });
        });

        await _bigFont.WaitAsync().ConfigureAwait(false);
        await _mediumFont.WaitAsync().ConfigureAwait(false);
        await _massiveFont.WaitAsync().ConfigureAwait(false);
        await Plugin.PluginInterface.UiBuilder.FontAtlas.BuildFontsAsync().ConfigureAwait(false);
    }
}