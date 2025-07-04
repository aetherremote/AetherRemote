using System.Numerics;
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
    TipService tipService,
    SpiralService spiralService) : IDrawable
{
    public void Draw()
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

        SharedUserInterfaces.ContentBox("StatusServerStatus", AetherRemoteStyle.PanelBackground, true, () =>
        {
            SharedUserInterfaces.MediumText("Server Status");
            ImGui.TextUnformatted("Connected");
        });

        if (SharedUserInterfaces.ContextBoxButton(FontAwesomeIcon.Plug, windowPadding, windowWidth))
            controller.Disconnect();

        SharedUserInterfaces.Tooltip("Disconnect");

        SharedUserInterfaces.ContentBox("StatusIdentity", AetherRemoteStyle.PanelBackground, true, () =>
        {
            SharedUserInterfaces.MediumText("Current Identity");
            ImGui.TextUnformatted(identityService.Identity);
        });

        if (SharedUserInterfaces.ContextBoxButton(FontAwesomeIcon.History, windowPadding, windowWidth))
            controller.ResetIdentity();

        SharedUserInterfaces.Tooltip("Reset identity");
        
        SharedUserInterfaces.ContentBox("StatusHypnosis", AetherRemoteStyle.PanelBackground, true, () =>
        {
            SharedUserInterfaces.MediumText("Hypnosis");
            ImGui.TextUnformatted(spiralService.IsBeingHypnotized 
                ? $"Spiral from {spiralService.Sender}"
                : "No active spirals");
        });
        
        if (SharedUserInterfaces.ContextBoxButton(FontAwesomeIcon.Stop, windowPadding, windowWidth))
            spiralService.StopCurrentSpiral();
        
        SharedUserInterfaces.Tooltip("Stop spiral");

        SharedUserInterfaces.ContentBox("StatusTips", AetherRemoteStyle.PanelBackground, false, () =>
        {
            SharedUserInterfaces.MediumText("Tips");
            ImGui.TextUnformatted(tipService.CurrentTip);
        });

        if (SharedUserInterfaces.ContextBoxButton(FontAwesomeIcon.Forward, windowPadding, windowWidth))
            tipService.NextTip();

        SharedUserInterfaces.Tooltip("Next Tip");

        ImGui.EndChild();
    }
}