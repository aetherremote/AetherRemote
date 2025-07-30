using System.Numerics;
using ImGuiNET;

namespace AetherRemoteClient.UI.Components.Input;

/// <summary>
///     UI Component to input four characters
/// </summary>
public class FourDigitInput(string id)
{
    /// <summary>
    ///     Get the width of the resulting code entry
    /// </summary>
    public static float Width => ImGui.GetFontSize() * 4 + ImGui.GetStyle().WindowPadding.Y * 3;

    /// <summary>
    ///     Gets the contents of the input
    /// </summary>
    public string Value => string.Join(string.Empty, _characters);

    // Labels for all four character input fields
    private readonly string[] _ids = [string.Concat("##1", id), string.Concat("##2", id), string.Concat("##3", id), string.Concat("##4", id)];
    
    // Track the four character input fields
    private readonly string[] _characters = [string.Empty, string.Empty, string.Empty, string.Empty];
    
    // Track if the four character input fields are focused or not
    private readonly bool[] _focused = [false, false, false, false];
    
    /// <summary>
    ///     Render the component
    /// </summary>
    public void Draw()
    {
        var size = ImGui.GetFontSize();
        DrawInput(0, size); ImGui.SameLine();
        DrawInput(1, size); ImGui.SameLine();
        DrawInput(2, size); ImGui.SameLine();
        DrawInput(3, size);
    }

    /// <summary>
    ///     Clears the contents of the input
    /// </summary>
    public void Clear()
    {
        for (var i = 0; i < _characters.Length; i++)
            _characters[i] = string.Empty;
    }

    private void DrawInput(int index, float width)
    {
        // Store cursor position to render text centered later
        var start = ImGui.GetCursorScreenPos();
        var height = ImGui.GetFrameHeight();

        // Draw the real input text
        ImGui.SetNextItemWidth(width);

        // Store if characters were changed
        bool clicked;

        // Check if the object was focused last friend
        if (_focused[index])
        {
            // Draw the input text normally
            clicked = ImGui.InputText(_ids[index], ref _characters[index], 1, ImGuiInputTextFlags.AutoSelectAll);
        }
        else
        {
            // Draw the input text, but without any color
            ImGui.PushStyleColor(ImGuiCol.Text, Vector4.Zero);
            clicked = ImGui.InputText(_ids[index], ref _characters[index], 1, ImGuiInputTextFlags.AutoSelectAll);
            ImGui.PopStyleColor();

            // Get the draw reference for this window
            var draw = ImGui.GetWindowDrawList();

            // Calculate the size of the character we're rendering
            var size = ImGui.CalcTextSize(_characters[index]);

            // Calculate where we should draw this
            var position = new Vector2(
                start.X + (width - size.X) * 0.5f,
                start.Y + (height - size.Y) * 0.5f
            );

            // Draw the inactive overlay
            draw.AddText(position, ImGui.GetColorU32(ImGuiCol.Text), _characters[index]);
        }

        // If the character input changed, backspace wasn't pressed, and this isn't the last one, focus the next item
        if (clicked && ImGui.IsKeyPressed(ImGuiKey.Backspace) is false && index is not 3)
            ImGui.SetKeyboardFocusHere();

        // Set focus
        _focused[index] = ImGui.IsItemActive();
    }
}