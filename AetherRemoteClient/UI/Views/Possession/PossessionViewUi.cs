using System.Numerics;
using AetherRemoteClient.Domain.Interfaces;
using AetherRemoteClient.Managers;
using AetherRemoteClient.Services;
using AetherRemoteClient.Style;
using AetherRemoteClient.UI.Components.Friends;
using AetherRemoteClient.Utils;
using Dalamud.Bindings.ImGui;

namespace AetherRemoteClient.UI.Views.Possession;

public class PossessionViewUi(
    FriendsListComponentUi friendsList,
    PossessionViewUiController controller,
    CommandLockoutService commandLockoutService,
    SelectionManager selectionManager): IDrawable
{
    public void Draw()
    {
        ImGui.BeginChild("PossessionContent", AetherRemoteStyle.ContentSize, false, AetherRemoteStyle.ContentFlags);
        
        SharedUserInterfaces.ContentBox("PossessionHeader", AetherRemoteColors.PanelColor, true, () =>
        {
            ImGui.TextWrapped("TODO, but be in render distance");
        });
        
        SharedUserInterfaces.ContentBox("PossessionButton", AetherRemoteColors.PanelColor, true, () =>
        {
            if (ImGui.Button("Possess", new Vector2(ImGui.GetWindowWidth() - AetherRemoteImGui.WindowPadding.X * 2, AetherRemoteDimensions.SendCommandButtonHeight)))
                controller.Possess();
        });
        
        SharedUserInterfaces.ContentBox("PossessionStop", AetherRemoteColors.PanelColor, true, () =>
        {
            if (ImGui.Button("Unpossess", new Vector2(ImGui.GetWindowWidth() - AetherRemoteImGui.WindowPadding.X * 2, AetherRemoteDimensions.SendCommandButtonHeight)))
                controller.Unpossess();
        });
        
        ImGui.EndChild();
        ImGui.SameLine();
        friendsList.Draw();
    }
}