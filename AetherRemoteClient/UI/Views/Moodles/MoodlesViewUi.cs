using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using AetherRemoteClient.Dependencies.Moodles.Domain;
using AetherRemoteClient.Domain.Interfaces;
using AetherRemoteClient.Managers;
using AetherRemoteClient.Services;
using AetherRemoteClient.Style;
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
    SelectionManager selectionManager) : IDrawable
{
    // Const
    private const int DefaultLinesPerMoodleTitle = 3;
    private const float IconSizeMultiplier = 0.5f;
    private const float DefaultMoodleIconSizeOffset = 8f * IconSizeMultiplier; // Debuffs have more spacing at the top
    private static readonly Vector2 DefaultMoodleButtonSize = new(94, 97);
    private static readonly Vector2 DefaultMoodleIconSize = new Vector2(48, 64) * IconSizeMultiplier;
    
    public void Draw()
    {
        ImGui.BeginChild("MoodlesContent", AetherRemoteStyle.ContentSize, false, AetherRemoteStyle.ContentFlags);

        if (AgreementsService.HasAgreedTo(AgreementsService.Agreements.MoodlesWarning))
            DrawContent();
        else
            DrawWarning();

        ImGui.EndChild();
        ImGui.SameLine();
        friendsList.Draw();
    }

    private void DrawContent()
    {
        var width = ImGui.GetWindowWidth();
        var padding = new Vector2(AetherRemoteImGui.WindowPadding.X, 0);

        var begin = ImGui.GetCursorPosY();
        SharedUserInterfaces.ContentBox("MoodlesFromMoodles", AetherRemoteStyle.PanelBackground, true, () =>
        {
            SharedUserInterfaces.MediumText("Select Moodle");

            ImGui.SetNextItemWidth(width - padding.X * 4 - ImGui.GetFontSize());
            ImGui.InputTextWithHint("##MoodlesSearchBar", "Search", ref controller.SearchTerm, 32);

            ImGui.SameLine();

            if (SharedUserInterfaces.IconButton(FontAwesomeIcon.Sync, null, "Refresh Moodles"))
                controller.RefreshMoodles();
        });
        
        var headerHeight = ImGui.GetCursorPosY() - begin;
        var moodlesContextBoxSize = new Vector2(0, ImGui.GetWindowHeight() - headerHeight - padding.X * 3 - AetherRemoteDimensions.SendCommandButtonHeight);
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
            var size = new Vector2(ImGui.GetWindowWidth() - padding.X * 2, AetherRemoteDimensions.SendCommandButtonHeight);
            if (selectionManager.Selected.Count is 0)
            {
                ImGui.BeginDisabled();
                ImGui.Button("You must select at least one friend", size);
                ImGui.EndDisabled();
            }
            else if (controller.MissingPermissionsForATarget())
            {
                ImGui.BeginDisabled();
                ImGui.Button("You lack permissions for one or more of your targets", size);
                ImGui.EndDisabled();
            }
            else
            {
                if (commandLockoutService.IsLocked)
                {
                    ImGui.BeginDisabled();
                    ImGui.Button("Send Moodle", size);
                    ImGui.EndDisabled();
                }
                else
                {
                    if (ImGui.Button("Send Moodle", size))
                        controller.SendMoodle();
                }
            }
        });
    }

    private void DrawWarning()
    {
        SharedUserInterfaces.ContentBox("MoodlesWarning", AetherRemoteColors.PrimaryColor, true, () =>
        {
            SharedUserInterfaces.BigTextCentered("Warning");
        });
        
        SharedUserInterfaces.ContentBox("MoodlesWarningText", AetherRemoteColors.PanelColor, true, () =>
        {
            ImGui.PushTextWrapPos(ImGui.GetWindowWidth() - AetherRemoteImGui.WindowPadding.X);
            ImGui.TextWrapped("There is currently an issue in Moodles preventing syncing services from properly updating. This most commonly results is Moodles being shown to other players despite them being right-clicked off.");
            ImGui.Spacing();
            ImGui.TextWrapped("There is nothing I can do about this, and it seemingly happens at random.");
            ImGui.Spacing();
            ImGui.TextWrapped("If you suspect a Moodle is there that shouldn't be, visit the Cleanup Tab inside of Moodles by enabling Debug Mode in the Settings. Otherwise, you need to acknowledge the possibility that this may happen.");
            ImGui.PopTextWrapPos();
        });
        
        SharedUserInterfaces.ContentBox("MoodlesWarningAccept", AetherRemoteColors.PanelColor, false, () =>
        {
            if (ImGui.Button("I understand the risks", new Vector2(ImGui.GetWindowWidth() - AetherRemoteImGui.WindowPadding.X * 2, AetherRemoteDimensions.SendCommandButtonHeight)))
                MoodlesViewUiController.AcceptMoodlesTermsOfService();
        });
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
            CenterWrapTextAndRender(screenPosition + padding, screenPosition + new Vector2(DefaultMoodleButtonSize.X, fontSize * 3) - padding, font, fontSize, moodle.PrettyTitle);
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
            ImGui.TextUnformatted(moodle.Info.ExpireTicks < 0 ? "Does not expire" : $"Expires in {moodle.PrettyExpiration}");
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