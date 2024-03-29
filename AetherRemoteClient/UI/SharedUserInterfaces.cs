using AetherRemoteClient.Domain;
using AetherRemoteCommon;
using Dalamud.Interface;
using Dalamud.Interface.Colors;
using Dalamud.Interface.ManagedFontAtlas;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using ImGuiNET;
using System.Numerics;
using System.Threading.Tasks;

namespace AetherRemoteClient.UI;

public class SharedUserInterfaces
{
    public static readonly ImGuiWindowFlags PopupWindowFlags =
        ImGuiWindowFlags.NoTitleBar |
        ImGuiWindowFlags.NoMove |
        ImGuiWindowFlags.NoResize;

    private readonly DalamudPluginInterface pluginInterface;
    private readonly IPluginLog logger;

    private static IFontHandle? BigFont;
    private static bool BigFontBuilt = false;
    private const int BigFontSize = 40;
    private const int BigFontDefaultOffset = 8;

    private static IFontHandle? MediumFont;
    private static bool MediumFontBuilt = false;
    private const int MediumFontSize = 24;
    private const int MediumFontDefaultOffset = -4;

    public SharedUserInterfaces(IPluginLog logger, DalamudPluginInterface pluginInterface)
    {
        this.pluginInterface = pluginInterface;
        this.logger = logger;

        Task.Run(BuildDefaultFontExtraSizes);
    }

    /// <summary>
    /// Draws an icon with optional color.
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
    /// Calculates the size a button would be. Useful for UI alignment.
    /// </summary>
    public static Vector2 CalculateIconButtonScaledSize(FontAwesomeIcon icon, float scale = 1.0f)
    {
        ImGui.PushFont(UiBuilder.IconFont);

        var size = ImGui.CalcTextSize(icon.ToIconString()) + (ImGui.GetStyle().FramePadding * 2);
        size.X = size.Y;
        size *= scale;

        ImGui.PopFont();

        return size;
    }

    /// <summary>
    /// Returns the size of an icon
    /// </summary>
    public static Vector2 CalculateIconSize(FontAwesomeIcon icon)
    {
        ImGui.PushFont(UiBuilder.IconFont);
        var size = ImGui.CalcTextSize(icon.ToIconString());
        ImGui.PopFont();
        return size;
    }

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
    /// Draws a button with an icon.
    /// </summary>
    public static bool IconButtonScaled(FontAwesomeIcon icon, float scale = 1.0f, string? id = null)
    {
        ImGui.PushFont(UiBuilder.IconFont);

        if (id != null)
            ImGui.PushID(id);

        var result = ImGui.Button(icon.ToIconString(), CalculateIconButtonScaledSize(icon, scale));

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
    /// Draws text using the big font with optional color.
    /// </summary>
    public static void BigText(string text, Vector4? color = null)
    {
        FontText(text, BigFont, BigFontBuilt, color);
    }

    /// <summary>
    /// Draws text using the default font, centered, with optional color.
    /// </summary>
    public static void TextCentered(string text, Vector4? color = null, int yOffset = 0)
    {
        FontTextCentered(text, null, false, yOffset, color);
    }

    /// <summary>
    /// Draws text using the medium font, centered, with optional color.
    /// </summary>
    public static void MediumTextCentered(string text, Vector4? color = null, int yOffset = 0)
    {
        FontTextCentered(text, MediumFont, MediumFontBuilt, yOffset, color);
    }

    /// <summary>
    /// Draws text using the big font, centered, with optional color.
    /// </summary>
    public static void BigTextCentered(string text, Vector4? color = null, int yOffset = 0)
    {
        FontTextCentered(text, BigFont, BigFontBuilt, yOffset, color);
    }

    /// <summary>
    /// Displays centered text with best fitting font size.
    /// </summary>
    public static void DynamicTextCentered(string text, float workingSpace, Vector4? color = null)
    {
        if (BigFontBuilt)
        {
            BigFont?.Push();
            var bigTextWidth = ImGui.CalcTextSize(text).X;
            BigFont?.Pop();

            if (bigTextWidth <= workingSpace)
            {
                BigTextCentered(text, color, BigFontDefaultOffset);
                return;
            }
        }

        if (MediumFontBuilt)
        {
            MediumFont?.Push();
            var mediumTextWidth = ImGui.CalcTextSize(text).X;
            MediumFont?.Pop();

            if (mediumTextWidth <= workingSpace)
            {
                MediumTextCentered(text, color, MediumFontDefaultOffset);
                return;
            }
        }

        TextCentered(text, color, 0);
    }

    public static bool MediumInputText(string id, string hint, ref string secretInputBoxText, ImGuiInputTextFlags flags = ImGuiInputTextFlags.None)
    {
        MediumFont?.Push();
        var inputText = ImGui.InputTextWithHint(id, hint, ref secretInputBoxText, 
            AetherRemoteConstants.SecretCharLimit, flags);
        MediumFont?.Pop();
        return inputText;
    }

    public static bool MediumButton(string id, string label, Vector2? size = null)
    {
        MediumFont?.Push();
        ImGui.PushID(id);
        var button = ImGui.Button(label, size ?? Vector2.Zero);
        MediumFont?.Pop();
        return button;
    }

    public static void PushMediumFont()
    {
        MediumFont?.Push();
    }

    public static void PopMediumFont()
    {
        MediumFont?.Pop();
    }

    public static void PushBigFont()
    {
        BigFont?.Push();
    }

    public static void PopBigFont()
    {
        BigFont?.Pop();
    }

    /// <summary>
    /// Draws text with a given color.
    /// </summary>
    public static void ColorText(string text, Vector4 color)
    {
        ImGui.PushStyleColor(ImGuiCol.Text, color);
        ImGui.TextUnformatted(text);
        ImGui.PopStyleColor();
    }

    /// <summary>
    /// Input text with searchable list.
    /// </summary>
    public static void ComboFilter(
        string id,
        ref string selectedString,
        ThreadedFilter<string> filterHelper,
        ImGuiWindowFlags? imGuiWindowFlags = null)
    {
        var _sizeX = 170;
        var _sizeY = filterHelper.List.Count < 10 ? (filterHelper.List.Count * 25) + 10 : 260;
        // TODO: Fix sizing bug when searching through options.
        // For example, when scanning through emotes, if you search "ma" you can see it clearly

        ImGui.SetNextItemWidth(_sizeX);
        if (ImGui.InputText($"##{id}-ComboFilter", ref selectedString, 32))
        {
            filterHelper.Restart(selectedString);
        }
        var isInputTextActive = ImGui.IsItemActive();
        var isInputTextActivated = ImGui.IsItemActivated();

        var popupName = $"##{id}-Popup";

        if (isInputTextActivated && !ImGui.IsPopupOpen(popupName))
        {
            ImGui.OpenPopup(popupName);
        }

        var _x = ImGui.GetItemRectMin().X;
        var _y = ImGui.GetCursorPosY() + ImGui.GetWindowPos().Y;
        ImGui.SetNextWindowPos(new Vector2(_x, _y));

        ImGui.SetNextWindowSize(new Vector2(_sizeX, _sizeY));

        if (ImGui.BeginPopup(popupName, imGuiWindowFlags ?? (PopupWindowFlags | ImGuiWindowFlags.ChildWindow)))
        {
            for (var i = 0; i < filterHelper.List.Count; i++)
            {
                var option = filterHelper.List[i];
                if (ImGui.Selectable(option))
                {
                    selectedString = option;
                }
            }

            if (!isInputTextActive && !ImGui.IsWindowFocused())
            {
                ImGui.CloseCurrentPopup();
            }

            ImGui.EndPopup();
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
    private static void FontTextCentered(string text, IFontHandle? font, bool fontBuilt, int yOffset = 0, Vector4? color = null)
    {
        if (fontBuilt) font?.Push();

        var userIdColor = color ?? ImGuiColors.DalamudWhite;
        var windowWidth = ImGui.GetWindowSize().X;
        var userIdWidth = ImGui.CalcTextSize(text).X;

        ImGui.SetCursorPosX((windowWidth - userIdWidth) * 0.5f);
        ImGui.SetCursorPosY(ImGui.GetCursorPosY() - yOffset);

        ColorText(text, userIdColor);

        if (fontBuilt) font?.Pop();
    }

    private async Task BuildDefaultFontExtraSizes()
    {
        BigFont = pluginInterface.UiBuilder.FontAtlas.NewDelegateFontHandle(toolkit => {
            toolkit.OnPreBuild(preBuild =>
            {
                preBuild.AddDalamudDefaultFont(BigFontSize);
            });
        });

        MediumFont = pluginInterface.UiBuilder.FontAtlas.NewDelegateFontHandle(toolkit => {
            toolkit.OnPreBuild(preBuild =>
            {
                preBuild.AddDalamudDefaultFont(MediumFontSize);
            });
        });

        await BigFont.WaitAsync();
        await MediumFont.WaitAsync();
        await pluginInterface.UiBuilder.FontAtlas.BuildFontsAsync();

        BigFontBuilt = true;
        MediumFontBuilt = true;
    }
}
