using System.Numerics;
using AetherRemoteClient.Domain.Interfaces;
using AetherRemoteClient.Services;
using AetherRemoteClient.Style;
using AetherRemoteClient.Utils;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
using Dalamud.Interface.Colors;

namespace AetherRemoteClient.UI.Views.Home;

public class HomeViewUi(HomeViewUiController controller, AccountService account, TipService tipService) : IDrawable
{
    public void Draw()
    {
        ImGui.BeginChild("HomeContent", Vector2.Zero, false, AetherRemoteImGui.ContentFlags);
        
        var windowWidth = ImGui.GetWindowWidth();

        SharedUserInterfaces.ContentBox("HomeHeader", AetherRemoteColors.PanelColor, true, () =>
        {
            SharedUserInterfaces.PushBigFont();
            
            var size = ImGui.CalcTextSize(account.FriendCode);
            ImGui.SetCursorPosX((windowWidth - size.X) * 0.5f);
            if (ImGui.Selectable(account.FriendCode, false, ImGuiSelectableFlags.None, size))
                ImGui.SetClipboardText(account.FriendCode);

            SharedUserInterfaces.PopBigFont();
            SharedUserInterfaces.TextCentered("(click friend code to copy)", ImGuiColors.DalamudGrey);
        });
        
        SharedUserInterfaces.ContentBox("HomeLogout", AetherRemoteColors.PanelColor, true, () =>
        {
            SharedUserInterfaces.MediumText("Welcome");
            ImGui.TextUnformatted(tipService.CurrentTip);
        });

        if (SharedUserInterfaces.ContextBoxButton(FontAwesomeIcon.Plug, AetherRemoteImGui.WindowPadding, windowWidth))
            _ = controller.Disconnect();
        
        SharedUserInterfaces.Tooltip("Disconnect");
        
        ImGui.EndChild();
    }
}