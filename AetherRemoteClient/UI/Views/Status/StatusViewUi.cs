using System.Numerics;
using AetherRemoteClient.Domain;
using AetherRemoteClient.Domain.Interfaces;
using AetherRemoteClient.Managers;
using AetherRemoteClient.Managers.Possession;
using AetherRemoteClient.Services;
using AetherRemoteClient.Style;
using AetherRemoteClient.UI.Components.Input;
using AetherRemoteClient.Utils;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
using Dalamud.Interface.Colors;

namespace AetherRemoteClient.UI.Views.Status;

public class StatusViewUi(
    StatusViewUiController controller,
    AccountService account,
    PermanentTransformationLockService permanentTransformationLockService,
    IdentityService identityService,
    TipService tipService,
    HypnosisManager hypnosisManager,
    PossessionManager possessionManager) : IDrawable
{
    public void Draw()
    {
        ImGui.BeginChild("SettingsContent", Vector2.Zero, false, AetherRemoteImGui.ContentFlags);
        
        var windowWidth = ImGui.GetWindowWidth();
        var windowPadding = ImGui.GetStyle().WindowPadding;

        ImGui.AlignTextToFramePadding();

        SharedUserInterfaces.ContentBox("StatusHeader", AetherRemoteColors.PanelColor, true, () =>
        {
            SharedUserInterfaces.PushBigFont();
            
            var size = ImGui.CalcTextSize(account.FriendCode);
            ImGui.SetCursorPosX((windowWidth - size.X) * 0.5f);
            if (ImGui.Selectable(account.FriendCode, false, ImGuiSelectableFlags.None, size))
                ImGui.SetClipboardText(account.FriendCode);

            SharedUserInterfaces.PopBigFont();
            SharedUserInterfaces.TextCentered("(click friend code to copy)", ImGuiColors.DalamudGrey);
        });
        
        SharedUserInterfaces.ContentBox("StatusLogout", AetherRemoteColors.PanelColor, true, () =>
        {
            SharedUserInterfaces.MediumText("Welcome");
            ImGui.TextUnformatted(tipService.CurrentTip);
        });

        if (SharedUserInterfaces.ContextBoxButton(FontAwesomeIcon.Plug, windowPadding, windowWidth))
            controller.Disconnect();
        
        SharedUserInterfaces.Tooltip("Disconnect");
        
        SharedUserInterfaces.ContentBox("StatusButtons", AetherRemoteColors.PanelColor, true, () =>
        {
            SharedUserInterfaces.MediumText("Statuses");
            ImGui.TextUnformatted("Various aspects of the plugin have lingering affects. You can find them below.");
            
            ImGui.SameLine();
            SharedUserInterfaces.Icon(FontAwesomeIcon.QuestionCircle);
            SharedUserInterfaces.Tooltip("Only active statuses will be displayed");
            
            SharedUserInterfaces.Tooltip(
                [
                    "Only active statuses will be displayed. Such statuses include:",
                    //"- Being permanently transformed",
                    //"- Being transformed",
                    //"- Being body swapped",
                    //"- Being twinned",
                    "- Being hypnotized"
                ]);
        });
        
        if (permanentTransformationLockService.Locked)
            RenderPermanentTransformationComponent(windowPadding, windowWidth);

        if (identityService.IsAltered)
            RenderTransformationComponent(windowPadding, windowWidth);

        if (hypnosisManager.IsBeingHypnotized)
            RenderHypnosisComponent(windowPadding, windowWidth);
        
        if (possessionManager.Possessed)
            RenderPossessionComponent(windowPadding, windowWidth);
        
        SharedUserInterfaces.ContentBox("OmniTool", AetherRemoteColors.PanelColor, false, () =>
        {
            SharedUserInterfaces.MediumText("Plugin Misbehaving? (Temporary Solution)");
            if (ImGui.Button("Reset Collection"))
                controller.ResetCollection();
            ImGui.SameLine();
            if (ImGui.Button("Reset Honorific"))
                controller.ResetHonorific();
            ImGui.SameLine();
            if (ImGui.Button("Reset Customize+"))
                controller.ResetCustomize();
        });
        
        ImGui.EndChild();
    }
    
    private void RenderPermanentTransformationComponent(Vector2 windowPadding, float windowWidth)
    {
        SharedUserInterfaces.ContentBox("StatusLock", AetherRemoteColors.PrimaryColor, true, () =>
        {
            SharedUserInterfaces.MediumText("Permanently Transformed");
            ImGui.TextUnformatted($"{identityService.Alteration?.Sender ?? "Unknown"} has locked your appearance");
        });

        var previousContextBoxSize = ImGui.GetItemRectSize();
        var endingCursorPosition = ImGui.GetCursorPosY();
        
        SharedUserInterfaces.PushBigFont();
        var cursorPositionStartX = windowWidth - previousContextBoxSize.Y - FourDigitInput.Width - windowPadding.Y * 2;
        var start = new Vector2(cursorPositionStartX ,endingCursorPosition - previousContextBoxSize.Y - windowPadding.Y * 2);
        ImGui.SetCursorPos(start);
        
        controller.PinInput.Draw();
        SharedUserInterfaces.PopBigFont();
        
        ImGui.SameLine();

        if (SharedUserInterfaces.IconButton(FontAwesomeIcon.Unlock, new Vector2(previousContextBoxSize.Y)))
            controller.Unlock();
        
        ImGui.SetCursorPosY(endingCursorPosition);
    }

    private void RenderTransformationComponent(Vector2 windowPadding, float windowWidth)
    {
        SharedUserInterfaces.ContentBox("StatusTransformation", AetherRemoteColors.PanelColor, true, () =>
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

        if (permanentTransformationLockService.Locked)
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
        SharedUserInterfaces.ContentBox("StatusHypnosis", AetherRemoteColors.PanelColor, true, () =>
        {
            SharedUserInterfaces.MediumText("Hypnosis");
            ImGui.TextUnformatted($"{hypnosisManager.Hypnotist?.NoteOrFriendCode ?? "Unknown"} is hypnotizing you");
        });
        
        if (SharedUserInterfaces.ContextBoxButton(FontAwesomeIcon.Square, windowPadding, windowWidth))
            hypnosisManager.Wake();
    }

    private void RenderPossessionComponent(Vector2 windowPadding, float windowWidth)
    {
        SharedUserInterfaces.ContentBox("StatusPossession", AetherRemoteColors.PanelColor, true, () =>
        {
            SharedUserInterfaces.MediumText("Possession");
            ImGui.TextUnformatted($"There is paranormal activity afoot...");
        });

        if (SharedUserInterfaces.ContextBoxButton(FontAwesomeIcon.Ghost, windowPadding, windowWidth, "Click to end possession", "PossessionId"))
            _ = controller.Unpossess();
    }
}
