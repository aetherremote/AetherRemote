using System.Numerics;
using AetherRemoteClient.Domain.Interfaces;
using AetherRemoteClient.Managers;
using AetherRemoteClient.Services;
using AetherRemoteClient.UI.Components.Friends;
using AetherRemoteClient.Utils;
using AetherRemoteCommon.Dependencies.Honorific.Domain;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
using Dalamud.Interface.ImGuiSeStringRenderer;
using Dalamud.Interface.Utility;
using Lumina.Text;

// ReSharper disable RedundantBoolCompare

namespace AetherRemoteClient.UI.Views.Honorific;

public class HonorificViewUi(HonorificViewUiController controller, FriendsListComponentUi friendsList, CommandLockoutService commandLockoutService, SelectionManager selectionManager) : IDrawable
{
    // Const
    private const int SendHonorificButtonHeight = 40;
    
    public void Draw()
    {
        ImGui.BeginChild("HonorificContent", AetherRemoteStyle.ContentSize, false, AetherRemoteStyle.ContentFlags);

        var width = ImGui.GetWindowWidth();
        var padding = new Vector2(ImGui.GetStyle().WindowPadding.X, 0);

        var begin = ImGui.GetCursorPosY();
        SharedUserInterfaces.ContentBox("TitleSearch", AetherRemoteStyle.PanelBackground, true, () =>
        {
            SharedUserInterfaces.MediumText("Select Title");

            ImGui.SetNextItemWidth(width - padding.X * 4 - ImGui.GetFontSize());
            ImGui.InputTextWithHint("##TitleSearchBar", "Search", ref controller.SearchTerm, 32);

            ImGui.SameLine();

            if (SharedUserInterfaces.IconButton(FontAwesomeIcon.Sync, null, "Refresh Titles"))
                controller.RefreshTitles();
        });
        
        var headerHeight = ImGui.GetCursorPosY() - begin;
        var honorificContextBoxSize = new Vector2(0, ImGui.GetWindowHeight() - headerHeight - padding.X * 3 - SendHonorificButtonHeight);
        if (ImGui.BeginChild("##HonorificContextBoxDisplay", honorificContextBoxSize, true, ImGuiWindowFlags.NoScrollbar))
        {
            var draw = ImGui.GetWindowDrawList();
            var parameters = new SeStringDrawParams
            {
                Color = 0xFFFFFFFF,
                WrapWidth = float.MaxValue,
                TargetDrawList = draw,
                Font = UiBuilder.DefaultFont,
                FontSize = UiBuilder.DefaultFontSizePx
            };
            
            foreach (var (character, titles) in controller.FilteredTitles)
            {
                if (ImGui.CollapsingHeader(character) is false)
                    continue;
                
                ImGui.PushStyleColor(ImGuiCol.Header, AetherRemoteStyle.PrimaryColor);
                foreach (var title in titles)
                    DrawTitleOption(draw, parameters, title);
                
                ImGui.PopStyleColor();
            }
            
            ImGui.EndChild();
        }

        ImGui.Spacing();
        
        SharedUserInterfaces.ContentBox("HonorificSend", AetherRemoteStyle.PanelBackground, false, () =>
        {
            if (selectionManager.Selected.Count is 0)
            {
                ImGui.BeginDisabled();
                ImGui.Button("You must select at least one friend", new Vector2(ImGui.GetWindowWidth() - padding.X * 2, SendHonorificButtonHeight));
                ImGui.EndDisabled();
            }
            else if (controller.MissingPermissionsForATarget())
            {
                ImGui.BeginDisabled();
                ImGui.Button("You lack permissions for one or more of your targets", new Vector2(ImGui.GetWindowWidth() - padding.X * 2, SendHonorificButtonHeight));
                ImGui.EndDisabled();
            }
            else if (controller.SelectedTitle is null)
            {
                ImGui.BeginDisabled();
                ImGui.Button("Select an Honorific", new Vector2(ImGui.GetWindowWidth() - padding.X * 2, SendHonorificButtonHeight));
                ImGui.EndDisabled();
            }
            else
            {
                if (commandLockoutService.IsLocked)
                {
                    ImGui.BeginDisabled();
                    ImGui.Button("Send Honorific", new Vector2(ImGui.GetWindowWidth() - padding.X * 2, SendHonorificButtonHeight));
                    ImGui.EndDisabled();
                }
                else
                {
                    if (ImGui.Button("Send Honorific", new Vector2(ImGui.GetWindowWidth() - padding.X * 2, SendHonorificButtonHeight)))
                        controller.SendHonorific();
                }
            }
        });
        
        ImGui.EndChild();
        ImGui.SameLine();
        friendsList.Draw();
    }

    private void DrawTitleOption(ImDrawListPtr draw, SeStringDrawParams parameters, HonorificInfo honorific)
    {
        var builder = new SeStringBuilder();

        if (honorific.Color is not null) builder.PushColorRgba(new Vector4(honorific.Color.Value, 1f));
        if (honorific.Glow is not null) builder.PushEdgeColorRgba(new Vector4(honorific.Glow.Value, 1f));
        builder.Append(honorific.Title == string.Empty ? "[Blank Title]" : honorific.Title);
        if (honorific.Glow is not null) builder.PopEdgeColor();
        if (honorific.Color is not null) builder.PopColor();

        draw.ChannelsSplit(2);
        draw.ChannelsSetCurrent(1);
        
        var bytes = Dalamud.Game.Text.SeStringHandling.SeString.Parse(builder.GetViewAsSpan()).Encode();
        ImGuiHelpers.SeStringWrapped(bytes, parameters);
        
        draw.ChannelsSetCurrent(0);
        if (ImGui.Selectable($"##{honorific.Title}", controller.SelectedTitle == honorific))
            controller.SelectedTitle = honorific;
        
        draw.ChannelsMerge();
    }
}