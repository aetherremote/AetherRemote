using System;
using System.IO;
using System.Numerics;
using System.Threading.Tasks;
using AetherRemoteClient.Domain;
using Dalamud.Interface;
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
    public const int HugeFontSize = 200;
    public const int BigFontSize = 40;
    public const int MediumFontSize = 24;

    private static readonly SafeFontConfig DefaultFontConfig = new() { SizePx = HugeFontSize };
    private static ImFontPtr _hugeFontPtr;
    private static IFontHandle? _hugeFont;
    private static bool _hugeFontBuilt;
    
    private static ImFontPtr _bigFontPtr;
    private static IFontHandle? _bigFont;
    private static bool _bigFontBuilt;

    private static ImFontPtr _mediumFontPtr;
    private static IFontHandle? _mediumFont;
    private static bool _mediumFontBuilt;

    /// <summary>
    ///     Draws a tool tip for the last hovered ImGui component
    /// </summary>
    /// <param name="tip"></param>
    public static void Tooltip(string tip)
    {
        if (ImGui.IsItemHovered() is false)
            return;

        ImGui.BeginTooltip();
        ImGui.Text(tip);
        ImGui.EndTooltip();
    }

    /// <summary>
    ///     Draws a tool tip for the last hovered ImGui component
    /// </summary>
    /// <param name="tips"></param>
    public static void Tooltip(string[] tips)
    {
        if (ImGui.IsItemHovered() is false)
            return;

        ImGui.BeginTooltip();
        foreach (var tip in tips)
            ImGui.Text(tip);

        ImGui.EndTooltip();
    }

    /// <summary>
    /// Draws a <see cref="FontAwesomeIcon"/>
    /// </summary>
    public static void Icon(FontAwesomeIcon icon, Vector4? color = null)
    {
        if (color.HasValue)
            ImGui.PushStyleColor(ImGuiCol.Text, color.Value);

        ImGui.PushFont(UiBuilder.IconFont);
        ImGui.TextUnformatted(icon.ToIconString());
        ImGui.PopFont();

        if (color.HasValue)
            ImGui.PopStyleColor();
    }

    /// <summary>
    ///     Creates a button with specified icon
    /// </summary>
    public static bool IconButton(FontAwesomeIcon icon, Vector2? size = null, string? tooltip = null)
    {
        ImGui.PushFont(UiBuilder.IconFont);
        var result = ImGui.Button(icon.ToIconString(), size ?? Vector2.Zero);
        ImGui.PopFont();

        if (tooltip is null)
            return result;

        if (ImGui.IsItemHovered() is false)
            return result;

        ImGui.BeginTooltip();
        ImGui.TextUnformatted(tooltip);
        ImGui.EndTooltip();

        return result;
    }

    /// <summary>
    ///     Draws text using the medium font with optional color.
    /// </summary>
    public static void MediumText(string text, Vector4? color = null)
    {
        if (color.HasValue)
            ImGui.PushStyleColor(ImGuiCol.Text, color.Value);

        if (_mediumFontBuilt)
            _mediumFont?.Push();

        ImGui.TextUnformatted(text);

        if (_mediumFontBuilt)
            _mediumFont?.Pop();

        if (color.HasValue)
            ImGui.PopStyleColor();
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
        if (_bigFontBuilt)
            _bigFont?.Push();

        ImGui.SetCursorPosX((ImGui.GetWindowWidth() - ImGui.CalcTextSize(text).X) * 0.5f);
        if (color is null)
            ImGui.TextUnformatted(text);
        else
            ImGui.TextColored(color.Value, text);

        if (_bigFontBuilt)
            _bigFont?.Pop();
    }

    /// <summary>
    ///     Draws text using the big font, centered, with optional color.
    /// </summary>
    public static void MediumTextCentered(string text, Vector4? color = null)
    {
        if (_mediumFontBuilt)
            _mediumFont?.Push();

        ImGui.SetCursorPosX((ImGui.GetWindowWidth() - ImGui.CalcTextSize(text).X) * 0.5f);
        if (color is null)
            ImGui.TextUnformatted(text);
        else
            ImGui.TextColored(color.Value, text);

        if (_mediumFontBuilt)
            _mediumFont?.Pop();
    }
    
    public static void PushHugeFont() => _hugeFont?.Push();
    public static void PopHugeFont() => _hugeFont?.Pop();
    public static void PushBigFont() => _bigFont?.Push();
    public static void PopBigFont() => _bigFont?.Pop();
    public static void PushMediumFont() => _mediumFont?.Push();
    public static void PopMediumFont() => _mediumFont?.Pop();
    public static ImFontPtr GetHugeFontPtr() => _hugeFontPtr;
    public static ImFontPtr GetBigFontPtr() => _bigFontPtr;
    public static ImFontPtr GetMediumFontPtr() => _mediumFontPtr;
    
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
    ///     Creates a rounded rectangle encompassing the current width of a child and the components inside the menu box
    /// </summary>
    public static void ContentBox(uint backgroundColor, Action contentToDraw, bool spanWindowWidth = true,
        bool addSpacingAtEnd = true)
    {
        var windowPadding = ImGui.GetStyle().WindowPadding;
        var drawList = ImGui.GetWindowDrawList();
        drawList.ChannelsSplit(2);
        drawList.ChannelsSetCurrent(1);

        var startPosition = ImGui.GetCursorPos();
        var anchorPoint = ImGui.GetCursorScreenPos();
        ImGui.SetCursorPos(startPosition + windowPadding);

        ImGui.BeginGroup();
        contentToDraw.Invoke();
        ImGui.EndGroup();

        drawList.ChannelsSetCurrent(0);

        var min = ImGui.GetItemRectMin() - windowPadding;
        var max = ImGui.GetItemRectMax() + windowPadding;
        if (spanWindowWidth)
            max.X = anchorPoint.X + ImGui.GetWindowWidth();

        ImGui.GetWindowDrawList().AddRectFilled(min, max, backgroundColor, AetherRemoteStyle.Rounding);
        drawList.ChannelsMerge();
        ImGui.SetCursorPosY(startPosition.Y + (max.Y - min.Y) + (addSpacingAtEnd ? windowPadding.Y : 0));
    }

    /// <summary>
    ///     Initializes the two additional font sizes used in the plugin
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
        
        _hugeFont = Plugin.PluginInterface.UiBuilder.FontAtlas.NewDelegateFontHandle(toolkit =>
        {
            toolkit.OnPreBuild(preBuild =>
            {
                _hugeFontPtr = dalamudFontDirectory is null 
                    ? preBuild.AddDalamudDefaultFont(HugeFontSize) 
                    : preBuild.AddFontFromFile(dalamudFontDirectory, DefaultFontConfig);
            });
        });
        
        _bigFont = Plugin.PluginInterface.UiBuilder.FontAtlas.NewDelegateFontHandle(toolkit =>
        {
            toolkit.OnPreBuild(preBuild => { _bigFontPtr = preBuild.AddDalamudDefaultFont(BigFontSize); });
        });

        _mediumFont = Plugin.PluginInterface.UiBuilder.FontAtlas.NewDelegateFontHandle(toolkit =>
        {
            toolkit.OnPreBuild(preBuild => { _mediumFontPtr = preBuild.AddDalamudDefaultFont(MediumFontSize); });
        });

        await _hugeFont.WaitAsync().ConfigureAwait(false);
        await _bigFont.WaitAsync().ConfigureAwait(false);
        await _mediumFont.WaitAsync().ConfigureAwait(false);
        await Plugin.PluginInterface.UiBuilder.FontAtlas.BuildFontsAsync().ConfigureAwait(false);

        _hugeFontBuilt = true;
        _bigFontBuilt = true;
        _mediumFontBuilt = true;
    }
}