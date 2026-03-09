using System.Numerics;
using AetherRemoteClient.Domain.Enums;
using AetherRemoteClient.Domain.Interfaces;
using AetherRemoteClient.Managers;
using AetherRemoteClient.Style;
using AetherRemoteClient.UI.Components.Friends;
using AetherRemoteClient.Utils;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface;

namespace AetherRemoteClient.UI.Views.Transformations.Views;

public partial class TransformationsViewUi(FriendsListComponentUi friendsList, Controllers.TransformationsViewUiController controller, SelectionManager selection) : IDrawable
{
    public void Draw()
    {
        if (ImGui.BeginChild("Transformations", AetherRemoteDimensions.ContentSize, false, AetherRemoteImGui.ContentFlags))
        {
            var width = ImGui.GetWindowWidth();
            var footerHeight = AetherRemoteDimensions.SendCommandButtonHeight + AetherRemoteImGui.WindowPadding.X * 2f;
            var headerButtonDimensions = new Vector2((width - AetherRemoteImGui.WindowPadding.X * 5) * 0.25f, 0);
            
            SharedUserInterfaces.ContentBox("TransformationsHeader", AetherRemoteColors.PanelColor, true, () =>
            {
                // Header
                SharedUserInterfaces.MediumTextCentered("Mode", width);
                ImGui.SameLine(width - ImGui.GetFontSize() - AetherRemoteImGui.WindowPadding.X * 2);
                SharedUserInterfaces.Icon(FontAwesomeIcon.QuestionCircle);
                
                // Snapshot the current mode
                var value = controller.Mode;
                if (value is TransformationMode.Transform) ImGui.PushStyleColor(ImGuiCol.Button, AetherRemoteColors.PrimaryColor);
                if (ImGui.Button("Transform", headerButtonDimensions))
                    controller.Mode = TransformationMode.Transform;
                if (value is TransformationMode.Transform) ImGui.PopStyleColor();
            
                ImGui.SameLine();
                
                if (value is TransformationMode.BodySwap) ImGui.PushStyleColor(ImGuiCol.Button, AetherRemoteColors.PrimaryColor);
                if (ImGui.Button("Body Swap", headerButtonDimensions))
                    controller.Mode = TransformationMode.BodySwap;
                if (value is TransformationMode.BodySwap) ImGui.PopStyleColor();
            
                ImGui.SameLine();
            
                if (value is TransformationMode.Twinning) ImGui.PushStyleColor(ImGuiCol.Button, AetherRemoteColors.PrimaryColor);
                if (ImGui.Button("Twinning", headerButtonDimensions))
                    controller.Mode = TransformationMode.Twinning;
                if (value is TransformationMode.Twinning) ImGui.PopStyleColor();
            
                ImGui.SameLine();
                
                if (value is TransformationMode.Mimicry) ImGui.PushStyleColor(ImGuiCol.Button, AetherRemoteColors.PrimaryColor);
                if (ImGui.Button("Mimicry", headerButtonDimensions))
                    controller.Mode = TransformationMode.Mimicry;
                if (value is TransformationMode.Mimicry) ImGui.PopStyleColor();
            });

            DrawTransformView(width, footerHeight);
            
            ImGui.Spacing();

            SharedUserInterfaces.ContentBox("TransformationSend", AetherRemoteColors.PanelColor, false, () =>
            {
                var size = new Vector2(width - AetherRemoteImGui.WindowPadding.X * 2, AetherRemoteDimensions.SendCommandButtonHeight);
                ImGui.Button("Send", size);
            });

            ImGui.EndChild();
            ImGui.SameLine();
            friendsList.Draw();
        }
    }
}