using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using AetherRemoteClient.Dependencies.Moodles.Domain;
using AetherRemoteClient.Domain.Interfaces;
using AetherRemoteClient.Services;
using AetherRemoteClient.UI.Components.Friends;
using AetherRemoteClient.Utils;
using AetherRemoteCommon.Dependencies.Moodles.Enums;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface;

namespace AetherRemoteClient.UI.Views.Moodles;

public class MoodlesViewUi(
    FriendsListComponentUi friendsList,
    MoodlesViewUiController controller,
    CommandLockoutService commandLockoutService,
    FriendsListService friendsListService) : IDrawable
{
    // Const
    private const int DefaultLinesPerMoodleTitle = 3;
    private const int SendMoodleButtonHeight = 40;
    private const float IconSizeMultiplier = 0.5f;
    private const float DefaultMoodleIconSizeOffset = 8f * IconSizeMultiplier; // Debuffs have more spacing at the top
    private static readonly Vector2 DefaultMoodleButtonSize = new(94, 97);
    private static readonly Vector2 DefaultMoodleIconSize = new Vector2(48, 64) * IconSizeMultiplier;
    
    public void Draw()
    {
        ImGui.BeginChild("MoodlesContent", AetherRemoteStyle.ContentSize, false, AetherRemoteStyle.ContentFlags);

        var topLeftCorner = ImGui.GetCursorScreenPos();

        var width = ImGui.GetWindowWidth();
        var padding = new Vector2(ImGui.GetStyle().WindowPadding.X, 0);

        var begin = ImGui.GetCursorPosY();
        SharedUserInterfaces.ContentBox("MoodlesFromMoodles", AetherRemoteStyle.PanelBackground, true, () =>
        {
            SharedUserInterfaces.MediumText("Select Moodles");

            ImGui.SetNextItemWidth(width - padding.X * 4 - ImGui.GetFontSize());
            ImGui.InputTextWithHint("##MoodlesSearchBar", "Search", ref controller.SearchTerm, 32);

            ImGui.SameLine();

            if (SharedUserInterfaces.IconButton(FontAwesomeIcon.Sync, null, "Refresh Moodles"))
                controller.RefreshMoodles();
        });
        
        var headerHeight = ImGui.GetCursorPosY() - begin;
        var moodlesContextBoxSize = new Vector2(0, ImGui.GetWindowHeight() - headerHeight - padding.X * 3 - SendMoodleButtonHeight);
        if (ImGui.BeginChild("##MoodlesContextBoxDisplay", moodlesContextBoxSize, true, ImGuiWindowFlags.NoScrollbar))
        {
            var maxMoodleButtonWidth = width - padding.X;
            var moodleButtonSpaceRemaining = maxMoodleButtonWidth;
            var increment = DefaultMoodleButtonSize.X + padding.X;
            
            var font = ImGui.GetFont();
            var fontSize = ImGui.GetFontSize();
            
            for (var i = 0; i < controller.FilteredMoodles.Count; i++)
            {
                DrawDisplayMoodleButton(controller.FilteredMoodles[i], i, padding, font, fontSize);
                
                if (moodleButtonSpaceRemaining - increment * 2 >= 0)
                {
                    ImGui.SameLine();
                    moodleButtonSpaceRemaining -= increment;
                }
                else
                {
                    ImGui.Spacing();
                    moodleButtonSpaceRemaining = maxMoodleButtonWidth;
                }
            }

            ImGui.EndChild();
        }

        ImGui.Spacing();
        
        SharedUserInterfaces.ContentBox("MoodlesSend", AetherRemoteStyle.PanelBackground, false, () =>
        {
            if (friendsListService.Selected.Count is 0)
            {
                ImGui.BeginDisabled();
                ImGui.Button("You must select at least one friend", new Vector2(ImGui.GetWindowWidth() - padding.X * 2, SendMoodleButtonHeight));
                ImGui.EndDisabled();
            }
            else if (controller.GetFriendsLackingPermissions().Count is not 0)
            {
                ImGui.BeginDisabled();
                ImGui.Button("You lack permissions for one or more of your targets", new Vector2(ImGui.GetWindowWidth() - padding.X * 2, SendMoodleButtonHeight));
                ImGui.EndDisabled();
            }
            else
            {
                if (commandLockoutService.IsLocked)
                {
                    ImGui.BeginDisabled();
                    ImGui.Button("Send Moodle", new Vector2(ImGui.GetWindowWidth() - padding.X * 2, SendMoodleButtonHeight));
                    ImGui.EndDisabled();
                }
                else
                {
                    if (ImGui.Button("Send Moodle", new Vector2(ImGui.GetWindowWidth() - padding.X * 2, SendMoodleButtonHeight)))
                        controller.SendMoodle();
                }
            }
        });

        ImGui.EndChild();
        ImGui.SameLine();
        friendsList.Draw();
    }
    
    private void DrawDisplayMoodleButton(Moodle moodle, int index, Vector2 padding, ImFontPtr font, float fontSize)
    {
        ImGui.BeginGroup();
        
        var start = ImGui.GetCursorPos();

        if (index == controller.SelectedMoodleIndex)
        {
            ImGui.PushStyleColor(ImGuiCol.Button, AetherRemoteStyle.PrimaryColor);
            ImGui.Button($"##{moodle.Info.Guid}", DefaultMoodleButtonSize);
            ImGui.PopStyleColor();
        }
        else
        {
            if (ImGui.Button($"##{moodle.Info.Guid}", DefaultMoodleButtonSize))
                controller.SelectedMoodleIndex = index;
        }
        
        if (MoodlesViewUiController.TryGetIcon(moodle.Info.IconId) is { } icon)
        {
            var x = start.X + (DefaultMoodleButtonSize.X - DefaultMoodleIconSize.X) * 0.5f;
            var y = start.Y + padding.X - (moodle.Info.Type is MoodleType.Positive ? 0 : DefaultMoodleIconSizeOffset);
            
            ImGui.SetCursorPos(new Vector2(x, y));
            ImGui.Image(icon.Handle, DefaultMoodleIconSize);

            var screenPosition = ImGui.GetCursorScreenPos();
            CenterWrapTextAndRender(screenPosition + padding, screenPosition + new Vector2(DefaultMoodleButtonSize.X, ImGui.GetFontSize() * 3) - padding, font, fontSize, moodle.PrettyTitle);
        }
        
        ImGui.EndGroup();
        
        // Tooltip
        // ReSharper disable once InvertIf
        if (ImGui.IsItemHovered())
        {
            ImGui.SetNextWindowSizeConstraints(new Vector2(300, 0), new Vector2(300, float.MaxValue));
            ImGui.BeginTooltip();
                
            SharedUserInterfaces.TextCentered(moodle.PrettyTitle);
            ImGui.Separator();
            ImGui.TextWrapped(moodle.PrettyDescription);
            ImGui.Separator();

            ImGui.TextUnformatted(moodle.Info.NoExpire 
                ? "Does not expire."
                : $"Expires in {moodle.Info.Days}d, {moodle.Info.Hours}h, {moodle.Info.Minutes}m, and {moodle.Info.Seconds}s");

            ImGui.EndTooltip();
        }
    }

    private static void CenterWrapTextAndRender(Vector2 begin, Vector2 end, ImFontPtr font, float fontSize, string text)
    {
        var boundingBox = end - begin;
        var maxLinesDouble = Math.Floor(boundingBox.Y / fontSize);
        var maxLines = maxLinesDouble > int.MaxValue ? DefaultLinesPerMoodleTitle : (int)maxLinesDouble;
        
        var wrappedLines = new List<string>();
        var currentLine = new StringBuilder();

        var words = text.Split(' ');
        foreach (var word in words)
        {
            var testLine = currentLine.Length is 0 ? word : currentLine + " " + word;
            var lineWidth = ImGui.CalcTextSizeA(font, fontSize, float.MaxValue, 0, testLine, out _).X;
            
            if (lineWidth <= boundingBox.X)
            {
                currentLine.Clear();
                currentLine.Append(testLine);
            }
            else
            {
                if (currentLine.Length is not 0)
                {
                    wrappedLines.Add(currentLine.ToString());
                }
                
                var partial = string.Empty;

                foreach (var letter in word)
                {
                    var check = partial + letter;
                    if (ImGui.CalcTextSizeA(font, fontSize, float.MaxValue, 0, check, out _).X > boundingBox.X)
                    {
                        wrappedLines.Add(check);
                        partial = letter.ToString();
                    }
                    else
                    {
                        partial = check;
                    }
                }

                currentLine.Clear();
                currentLine.Append(partial);
            }
        }
        
        if (currentLine.Length > 0)
            wrappedLines.Add(currentLine.ToString());

        if (wrappedLines.Count > maxLines)
        {
            wrappedLines = wrappedLines[..maxLines];

            var lastLine = wrappedLines[^1];
            if (lastLine.Length < 3)
            {
                wrappedLines[^1] = "...";
            }
            else
            {
                wrappedLines[^1] = lastLine[..3] + "...";
            }
        }

        for (var i = 0; i < wrappedLines.Count; i++)
        {
            var line = wrappedLines[i];
            var textWidth = ImGui.CalcTextSizeA(font, fontSize, float.MaxValue, 0, line, out _).X;

            var x = begin.X + boundingBox.X * 0.5f - textWidth * 0.5f;
            var y = begin.Y + i * fontSize;
            
            ImGui.SetCursorScreenPos(new Vector2(x, y));
            ImGui.Text(line);
        }
    }
}