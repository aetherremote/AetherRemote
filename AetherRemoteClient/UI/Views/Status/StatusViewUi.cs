using System.Numerics;
using AetherRemoteClient.Domain;
using AetherRemoteClient.Domain.Interfaces;
using AetherRemoteClient.Services;
using AetherRemoteClient.Utils;
using Dalamud.Interface;
using Dalamud.Interface.Colors;
using ImGuiNET;

namespace AetherRemoteClient.UI.Views.Status;

public class StatusViewUi(
    StatusViewUiController controller,
    IdentityService identityService,
    PermanentLockService permanentLockService,
    TipService tipService,
    SpiralService spiralService) : IDrawable
{
    private const int Size = SharedUserInterfaces.BigFontSize;
    private const float ButtonSpacing = 8f;
    
    private bool _drawStatusMenu = false;

    public void Draw()
    {
        if (_drawStatusMenu)
            DrawStatusMenu();
        else
            DrawUnlockMenu();
    }

    private void DrawStatusMenu()
    {
        ImGui.BeginChild("SettingsContent", Vector2.Zero, false, AetherRemoteStyle.ContentFlags);
        
        var windowWidth = ImGui.GetWindowWidth();
        var windowPadding = ImGui.GetStyle().WindowPadding;

        ImGui.AlignTextToFramePadding();

        SharedUserInterfaces.ContentBox("StatusHeader", AetherRemoteStyle.PanelBackground, true, () =>
        {
            SharedUserInterfaces.PushBigFont();

            var friendCode = identityService.FriendCode;
            var size = ImGui.CalcTextSize(friendCode);

            ImGui.SetCursorPosX((windowWidth - size.X) * 0.5f);
            if (ImGui.Selectable(friendCode, false, ImGuiSelectableFlags.None, size))
                ImGui.SetClipboardText(friendCode);

            SharedUserInterfaces.PopBigFont();
            SharedUserInterfaces.TextCentered("(click friend code to copy)", ImGuiColors.DalamudGrey);
        });
        
        SharedUserInterfaces.ContentBox("StatusTips", AetherRemoteStyle.PanelBackground, true, () =>
        {
            SharedUserInterfaces.TextCentered(tipService.CurrentTip);
        });
        
        SharedUserInterfaces.ContentBox("StatusButtons", AetherRemoteStyle.PanelBackground, true, () =>
        {
            SharedUserInterfaces.MediumText("Statuses");
            ImGui.TextUnformatted("Below are all the statuses affecting you, if any");
        });
        
        if (permanentLockService.IsLocked)
            RenderPermanentTransformationComponent(windowPadding, windowWidth);

        if (identityService.IsAltered)
            RenderTransformationComponent(windowPadding, windowWidth);

        if (spiralService.IsBeingHypnotized)
            RenderHypnosisComponent(windowPadding, windowWidth);

        
        ImGui.EndChild();
    }
    
    private void DrawUnlockMenu()
    {
        ImGui.BeginChild("SettingsContent", Vector2.Zero, false, AetherRemoteStyle.ContentFlags);
        
        ImGui.AlignTextToFramePadding();
        
        SharedUserInterfaces.ContentBox("LockHeader", AetherRemoteStyle.PanelBackground, true, () =>
        {
            if (SharedUserInterfaces.IconButton(FontAwesomeIcon.ArrowLeft, new Vector2(40), "Back"))
                _drawStatusMenu = true;
            
            ImGui.SameLine();
            
            SharedUserInterfaces.PushBigFont();
            SharedUserInterfaces.TextCentered("Unlock");
            SharedUserInterfaces.PopBigFont();
            SharedUserInterfaces.TextCentered("Ask your friend for the key or use safe mode to unlock your appearance");
        });
        
        SharedUserInterfaces.ContentBox("LockBody", AetherRemoteStyle.PanelBackground, true, () =>
        {
            var padding = ImGui.GetStyle().FramePadding;
            var window = ImGui.GetStyle().WindowPadding;
            
            var half = ImGui.GetWindowWidth() * 0.5f;

            var keyInputTextSize = (4 * Size) + (2 * padding.X) + (2 * window.X);
            
            ImGui.BeginGroup();
            SharedUserInterfaces.MediumText("Note:");
            
            ImGui.PushTextWrapPos(half - keyInputTextSize * 0.5f - padding.X * 2);
            ImGui.TextColored(ImGuiColors.DalamudGrey, "Alphanumeric and special characters can be used");
            ImGui.PopTextWrapPos();
            
            ImGui.EndGroup();
            
            ImGui.SameLine();
            
            
            
            SharedUserInterfaces.PushBigFont();
            
            
            ImGui.SetCursorPosX(half - keyInputTextSize * 0.5f);
            
            ImGui.BeginGroup();

            RenderCenteredTextInput(ref controller.KeyCharacters[0], ref _focused[0], "111",Size); ImGui.SameLine();
            RenderCenteredTextInput(ref controller.KeyCharacters[1], ref _focused[1], "222",  Size); ImGui.SameLine();
            RenderCenteredTextInput(ref controller.KeyCharacters[2], ref _focused[2],  "333", Size); ImGui.SameLine();
            RenderCenteredTextInput(ref controller.KeyCharacters[3], ref _focused[3], "444",Size);
            
            SharedUserInterfaces.PopBigFont();
            
            ImGui.Spacing();
            
            ImGui.SetCursorPosX(half - keyInputTextSize * 0.5f);
            
            var buttonSize = new Vector2((keyInputTextSize - ButtonSpacing * 2) / 3);

            ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, new Vector2(ButtonSpacing));
            ImGui.Button($"1##1", buttonSize); ImGui.SameLine();
            ImGui.Button($"2##2", buttonSize); ImGui.SameLine();
            ImGui.Button($"3##3", buttonSize);
            
            ImGui.Button($"4##4", buttonSize); ImGui.SameLine();
            ImGui.Button($"5##5", buttonSize); ImGui.SameLine();
            ImGui.Button($"6##6", buttonSize);
            
            ImGui.Button($"7##7", buttonSize); ImGui.SameLine();
            ImGui.Button($"8##8", buttonSize); ImGui.SameLine();
            ImGui.Button($"9##9", buttonSize);
            
            SharedUserInterfaces.IconButton(FontAwesomeIcon.Backspace, buttonSize); ImGui.SameLine();
            ImGui.Button($"0##0", buttonSize); ImGui.SameLine();
            SharedUserInterfaces.IconButton(FontAwesomeIcon.Unlock, buttonSize);
            
            ImGui.PopStyleVar();

            ImGui.Dummy(new Vector2(ImGui.GetWindowHeight() - ImGui.GetCursorPosY()) - ImGui.GetStyle().WindowPadding);
            
            ImGui.EndGroup();
        });
        
        ImGui.EndChild();
    }

    private readonly bool[] _focused = new bool[4];
    
    private static void RenderCenteredTextInput(ref string text, ref bool focused, string label, float width = 200f)
    {
        // Push Label
        ImGui.PushID(label);
        
        // Store cursor position to render text centered later
        var start = ImGui.GetCursorScreenPos();
        var height = ImGui.GetFrameHeight();

        // Intercept the referenced text so we can manipulate the value passed to the input text
        var intercept = focused ? text : string.Empty;
        
        // Draw the real input text
        ImGui.SetNextItemWidth(width);
        ImGui.InputText("##UnlockInput", ref intercept, 1, ImGuiInputTextFlags.AutoSelectAll);

        // If item is active store the 
        if (ImGui.IsItemActive())
        {
            // Restore the text now that we intercepted
            text = intercept;
        }
        else
        {
            // Get the draw reference for this window
            var draw = ImGui.GetWindowDrawList();

            // Calculate the size of the character we're rendering
            var size = ImGui.CalcTextSize(text);
            
            // Calculate where we should draw this
            var position = new Vector2(
                start.X + (width - size.X) * 0.5f,
                start.Y + (height - size.Y) * 0.5f
            );
            
            // Draw the inactive overlay
            draw.AddText(position, ImGui.GetColorU32(ImGuiCol.Text), text);
        }
        
        // Pop the ID now that we're done with it
        ImGui.PopID();
    }

    private void RenderPermanentTransformationComponent(Vector2 windowPadding, float windowWidth)
    {
        SharedUserInterfaces.ContentBox("StatusLock", AetherRemoteStyle.ElevatedBackground, true, () =>
        {
            SharedUserInterfaces.MediumText("Permanently Transformed");
            ImGui.TextUnformatted($"{identityService.Alteration?.Sender ?? "Unknown"} has locked your appearance");
        });
        
        if (SharedUserInterfaces.ContextBoxButton(FontAwesomeIcon.Unlock, windowPadding, windowWidth))
        {
            _drawStatusMenu = false;
        }
    }

    private void RenderTransformationComponent(Vector2 windowPadding, float windowWidth)
    {
        SharedUserInterfaces.ContentBox("StatusTransformation", AetherRemoteStyle.PanelBackground, true, () =>
        {
            SharedUserInterfaces.MediumText("Identity Altered");

            if (identityService.Alteration is not { } alteration)
            {
                ImGui.TextUnformatted("An unknown friend altered your identity");
                return;
            }

            var type = alteration.Type switch
            {
                IdentityAlterationType.Transformation => $"{alteration.Sender} transformed you or your clothing",
                IdentityAlterationType.Twinning => $"{alteration.Sender} twinned with you",
                IdentityAlterationType.BodySwap => $"{alteration.Sender} swapped your body",
                _ => $"{alteration.Sender} altered your identity"
            };
            
            ImGui.TextUnformatted(type);
        });

        if (permanentLockService.IsLocked)
        {
            ImGui.BeginDisabled();
            SharedUserInterfaces.ContextBoxButton(FontAwesomeIcon.History, windowPadding, windowWidth);
            ImGui.EndDisabled();
        }
        else
        {
            if (SharedUserInterfaces.ContextBoxButton(FontAwesomeIcon.History, windowPadding, windowWidth))
                controller.ResetIdentity();
        }
    }

    private void RenderHypnosisComponent(Vector2 windowPadding, float windowWidth)
    {
        SharedUserInterfaces.ContentBox("StatusHypnosis", AetherRemoteStyle.PanelBackground, true, () =>
        {
            SharedUserInterfaces.MediumText("Hypnosis");
            ImGui.TextUnformatted($"{identityService.Alteration?.Sender ?? "Unknown"} is hypnotizing you");
        });
        
        if (SharedUserInterfaces.ContextBoxButton(FontAwesomeIcon.Square, windowPadding, windowWidth))
            spiralService.StopCurrentSpiral();
    }
}