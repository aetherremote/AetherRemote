using System.Numerics;
using AetherRemoteClient.Domain.Interfaces;
using AetherRemoteClient.Managers;
using AetherRemoteClient.Style;
using AetherRemoteClient.UI.Components.Friends;
using AetherRemoteClient.Utils;
using Dalamud.Bindings.ImGui;

namespace AetherRemoteClient.UI.Views.Friends;

public partial class FriendsViewUi(FriendsListComponentUi friendsList, FriendsViewUiController controller, SelectionManager selection) : IDrawable
{
    private bool _drawIndividuals = true;
    
    public void Draw()
    {
        ImGui.BeginChild("PermissionContent", AetherRemoteStyle.ContentSize, false, AetherRemoteStyle.ContentFlags);

        var width = ImGui.GetWindowWidth();
        
        SharedUserInterfaces.ContentBox("PermissionsSelectMode", AetherRemoteStyle.PanelBackground, true, () =>
        {
            var buttonWidth = (width - 3 * AetherRemoteImGui.WindowPadding.X) / 2f;
            var buttonDimensions = new Vector2(buttonWidth, AetherRemoteDimensions.SendCommandButtonHeight);
            
            SharedUserInterfaces.PushMediumFont();
            SharedUserInterfaces.TextCentered("Permissions");
            SharedUserInterfaces.PopMediumFont();
            
            if (_drawIndividuals)
            {
                ImGui.PushStyleColor(ImGuiCol.Button, AetherRemoteStyle.PrimaryColor);
                ImGui.Button("Individual", buttonDimensions);
                ImGui.PopStyleColor();
                
                ImGui.SameLine();
                
                if (ImGui.Button("Global", buttonDimensions))
                    _drawIndividuals = false;
            }
            else
            {
                if (ImGui.Button("Individual", buttonDimensions))
                    _drawIndividuals = true;
                
                ImGui.SameLine();
                
                ImGui.PushStyleColor(ImGuiCol.Button, AetherRemoteStyle.PrimaryColor);
                ImGui.Button("Global", buttonDimensions);
                ImGui.PopStyleColor();
            }
        });
        
        if (_drawIndividuals)
            DrawIndividualPermissions(width);
        else
            DrawGlobalPermissions(width);
        
        ImGui.EndChild();
        ImGui.SameLine();
        friendsList.Draw(true, true);
    }
}