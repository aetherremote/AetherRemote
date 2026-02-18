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
