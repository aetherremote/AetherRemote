using System.Numerics;
using AetherRemoteClient.Domain.Interfaces;
using AetherRemoteClient.Ipc;
using AetherRemoteClient.Services;
using AetherRemoteClient.Utils;
using Dalamud.Interface;
using Dalamud.Interface.Colors;
using ImGuiNET;

namespace AetherRemoteClient.UI.Views.Status;

public class StatusViewUi(
    NetworkService networkService,
    IdentityService identityService,
    TipService tipService,
    SpiralService spiralService,
    GlamourerIpc glamourerIpc) : IDrawable
{
    private readonly StatusViewUiController _controller = new(networkService, identityService, glamourerIpc);

    public bool Draw()
    {
        ImGui.BeginChild("SettingsContent", Vector2.Zero, false, AetherRemoteStyle.ContentFlags);

        var windowWidth = ImGui.GetWindowWidth();
        var windowPadding = ImGui.GetStyle().WindowPadding;

        ImGui.AlignTextToFramePadding();

        SharedUserInterfaces.ContentBox(AetherRemoteStyle.PanelBackground, () =>
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

        SharedUserInterfaces.ContentBox(AetherRemoteStyle.PanelBackground, () =>
        {
            SharedUserInterfaces.MediumText("Server Status");
            ImGui.TextUnformatted("Connected");
        });

        if (SharedUserInterfaces.ContextBoxButton(FontAwesomeIcon.Plug, windowPadding, windowWidth))
            _controller.Disconnect();

        SharedUserInterfaces.Tooltip("Disconnect");

        SharedUserInterfaces.ContentBox(AetherRemoteStyle.PanelBackground, () =>
        {
            SharedUserInterfaces.MediumText("Current Identity");
            ImGui.TextUnformatted(identityService.Identity);
        });

        if (SharedUserInterfaces.ContextBoxButton(FontAwesomeIcon.History, windowPadding, windowWidth))
            _controller.ResetIdentity();

        SharedUserInterfaces.Tooltip("Reset identity");
        
        SharedUserInterfaces.ContentBox(AetherRemoteStyle.PanelBackground, () =>
        {
            SharedUserInterfaces.MediumText("Hypnosis");
            ImGui.TextUnformatted(spiralService.IsBeingHypnotized 
                ? $"Spiral from {spiralService.Sender}"
                : "No active spirals");
        });
        
        if (SharedUserInterfaces.ContextBoxButton(FontAwesomeIcon.Stop, windowPadding, windowWidth))
            spiralService.StopCurrentSpiral();
        
        SharedUserInterfaces.Tooltip("Stop spiral");

        SharedUserInterfaces.ContentBox(AetherRemoteStyle.PanelBackground, () =>
        {
            SharedUserInterfaces.MediumText("Tips");
            ImGui.TextUnformatted(tipService.CurrentTip);
        });

        if (SharedUserInterfaces.ContextBoxButton(FontAwesomeIcon.Forward, windowPadding, windowWidth))
            tipService.NextTip();

        SharedUserInterfaces.Tooltip("Next Tip");

        ImGui.EndChild();
        return false;
    }
}