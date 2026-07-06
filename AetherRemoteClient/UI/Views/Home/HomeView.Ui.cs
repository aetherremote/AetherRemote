using System.Numerics;
using AetherRemoteClient.UI.Style;
using AetherRemoteClient.Utils;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
using Dalamud.Interface.Colors;

namespace AetherRemoteClient.UI.Views.Home;

public partial class HomeView
{
    public void Draw()
    {
        ImGui.BeginChild("HomeContent", Vector2.Zero, false, AetherRemoteImGui.ContentFlags);
        
        var windowWidth = ImGui.GetWindowWidth();

        SharedUserInterfaces.ContentBox("HomeHeader", AetherRemoteColors.PanelColor, true, () =>
        {
            SharedUserInterfaces.PushBigFont();

            var friendCode = _activeSessionService.FriendCode ?? "<Unknown>";
            var size = ImGui.CalcTextSize(friendCode);
            ImGui.SetCursorPosX((windowWidth - size.X) * 0.5f);
            if (ImGui.Selectable(friendCode, false, ImGuiSelectableFlags.None, size))
                ImGui.SetClipboardText(friendCode);

            SharedUserInterfaces.PopBigFont();
            SharedUserInterfaces.TextCentered("(click friend code to copy)", ImGuiColors.DalamudGrey);
        });
        
        SharedUserInterfaces.ContentBox("HomeLogout", AetherRemoteColors.PanelColor, true, () =>
        {
            SharedUserInterfaces.MediumText("Welcome");
            ImGui.TextUnformatted(_tipService.CurrentTip);
        });

        if (SharedUserInterfaces.ContextBoxButton(FontAwesomeIcon.Plug, AetherRemoteImGui.WindowPadding, windowWidth))
            _ = Disconnect();
        
        SharedUserInterfaces.Tooltip("Disconnect");
        
        SharedUserInterfaces.ContentBox("HomeTutorial", AetherRemoteColors.PanelColor, true, () =>
        {
            SharedUserInterfaces.MediumText("Tutorial");
            ImGui.TextUnformatted("(WIP) A compendium of information on the plugin");
        });

        if (SharedUserInterfaces.ContextBoxButton(FontAwesomeIcon.Book, AetherRemoteImGui.WindowPadding, windowWidth))
            _showTutorialWindow = true;
        
        SharedUserInterfaces.Tooltip("Open Tutorial");
        
        if (_showTutorialWindow)
            DrawTutorialWindow();
        
        ImGui.EndChild();
    }

    private void DrawTutorialWindow()
    {
        if (ImGui.Begin("Aether Remote Tutorial", ref _showTutorialWindow))
        {
            SharedUserInterfaces.MediumText("(WIP) Tutorial");
            ImGui.TextWrapped("I will be updating this slowly as time permits. If you would like to help contribute to the tutorial, reach out to me, and we can find a place for you to document. If you have a preference, let me know. You will be credited in the bottom of whichever section you work on.");
        }
        
        ImGui.End();
    }
}