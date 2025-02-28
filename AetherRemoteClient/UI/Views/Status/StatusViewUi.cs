using System.Numerics;
using AetherRemoteClient.Domain.Interfaces;
using AetherRemoteClient.Services;
using AetherRemoteClient.Services.External;
using AetherRemoteClient.Utils;
using Dalamud.Interface;
using Dalamud.Interface.Colors;
using ImGuiNET;

namespace AetherRemoteClient.UI.Views.Status;

public class StatusViewUi(GlamourerService glamourerService, NetworkService networkService, IdentityService identityService) : IDrawable
{
    private readonly StatusViewUiController _controller = new(glamourerService, networkService, identityService);
    
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
        
        ImGui.EndChild();
        return false;
    }

    
}