using System.Numerics;
using AetherRemoteClient.Domain.Interfaces;
using AetherRemoteClient.Managers;
using AetherRemoteClient.Services;
using AetherRemoteClient.Style;
using AetherRemoteClient.UI.Components.Friends;
using AetherRemoteClient.Utils;
using AetherRemoteCommon.Dependencies.Honorific.Domain;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
using Dalamud.Interface.ImGuiSeStringRenderer;
using Dalamud.Interface.Utility;
using Lumina.Text;

namespace AetherRemoteClient.UI.Views.Honorific;

public class HonorificViewUi(HonorificViewUiController controller, FriendsListComponentUi friendsList, CommandLockoutService commandLockoutService, SelectionManager selectionManager) : IDrawable
{
    public void Draw()
    {
        ImGui.BeginChild("HonorificContent", AetherRemoteDimensions.ContentSize, false, AetherRemoteImGui.ContentFlags);

        var width = ImGui.GetWindowWidth();

        var begin = ImGui.GetCursorPosY();
        SharedUserInterfaces.ContentBox("TitleSearch", AetherRemoteColors.PanelColor, true, () =>
        {
            SharedUserInterfaces.MediumText("Select Title");

            ImGui.SetNextItemWidth(width - AetherRemoteImGui.WindowPadding.X * 4 - ImGui.GetFontSize());
            ImGui.InputTextWithHint("##TitleSearchBar", "Search", ref controller.SearchTerm, 32);

            ImGui.SameLine();

            if (SharedUserInterfaces.IconButton(FontAwesomeIcon.Sync, null, "Refresh Titles"))
                controller.RefreshTitles();
        });
        
        var headerHeight = ImGui.GetCursorPosY() - begin;
        var honorificContextBoxSize = new Vector2(0, ImGui.GetWindowHeight() - headerHeight - AetherRemoteImGui.WindowPadding.X * 3 - AetherRemoteDimensions.SendCommandButtonHeight);
        if (ImGui.BeginChild("##HonorificContextBoxDisplay", honorificContextBoxSize, true, ImGuiWindowFlags.NoScrollbar))
        {
            var parameters = new SeStringDrawParams
            {
                Color = 0xFFFFFFFF,
                WrapWidth = float.MaxValue,
                TargetDrawList = ImGui.GetWindowDrawList(),
                Font = UiBuilder.DefaultFont,
                FontSize = UiBuilder.DefaultFontSizePx
            };
            
            foreach (var (character, titles) in controller.FilteredTitles)
            {
                if (ImGui.CollapsingHeader(character) is false)
                    continue;
                
                ImGui.PushStyleColor(ImGuiCol.Header, AetherRemoteColors.PrimaryColor);
                foreach (var title in titles)
                    DrawTitleOption(parameters, title);
                
                ImGui.PopStyleColor();
            }
            
            ImGui.EndChild();
        }

        ImGui.Spacing();
        
        SharedUserInterfaces.ContentBox("HonorificSend", AetherRemoteColors.PanelColor, false, () =>
        {
            var size = new Vector2(ImGui.GetWindowWidth() - AetherRemoteImGui.WindowPadding.X * 2, AetherRemoteDimensions.SendCommandButtonHeight);
            if (selectionManager.Selected.Count is 0)
            {
                ImGui.BeginDisabled();
                ImGui.Button("You must select at least one friend", size);
                ImGui.EndDisabled();
            }
            else if (controller.MissingPermissionsForATarget())
            {
                ImGui.BeginDisabled();
                ImGui.Button("You lack permissions for one or more of your targets", size);
                ImGui.EndDisabled();
            }
            else if (controller.SelectedTitle is null)
            {
                ImGui.BeginDisabled();
                ImGui.Button("Select an Honorific", size);
                ImGui.EndDisabled();
            }
            else
            {
                if (commandLockoutService.IsLocked)
                {
                    ImGui.BeginDisabled();
                    ImGui.Button("Send Honorific", size);
                    ImGui.EndDisabled();
                }
                else
                {
                    if (ImGui.Button("Send Honorific", size))
                        controller.SendHonorific();
                }
            }
        });
        
        ImGui.EndChild();
        ImGui.SameLine();
        friendsList.Draw();
    }

    private void DrawTitleOption(SeStringDrawParams parameters, HonorificInfo honorific)
    {
        var builder = new SeStringBuilder();

        if (honorific.Color is not null) builder.PushColorRgba(new Vector4(honorific.Color, 1f));
        if (honorific.Glow is not null) builder.PushEdgeColorRgba(new Vector4(honorific.Glow, 1f));
        builder.Append(honorific.Title == string.Empty ? "[Blank Title]" : honorific.Title);
        if (honorific.Glow is not null) builder.PopEdgeColor();
        if (honorific.Color is not null) builder.PopColor();
        
        parameters.ScreenOffset = ImGui.GetCursorScreenPos();
        
        if (ImGui.Selectable($"##{honorific.Title}", controller.SelectedTitle == honorific))
            controller.SelectedTitle = honorific;
        
        var bytes = Dalamud.Game.Text.SeStringHandling.SeString.Parse(builder.GetViewAsSpan()).Encode();
        ImGuiHelpers.SeStringWrapped(bytes, parameters);
    }
}