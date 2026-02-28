using System.Numerics;
using AetherRemoteClient.Domain.Interfaces;
using AetherRemoteClient.Managers;
using AetherRemoteClient.Services;
using AetherRemoteClient.Style;
using AetherRemoteClient.Utils;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface;

namespace AetherRemoteClient.UI.Views.Status;

public class StatusViewUi(StatusViewUiController controller, StatusManager statusManager) : IDrawable
{
    public void Draw()
    {
        ImGui.BeginChild("SpeakContent", Vector2.Zero, false, AetherRemoteImGui.ContentFlags);
        
        SharedUserInterfaces.ContentBox("StatusHeader", AetherRemoteColors.PanelColor, true, () =>
        {
            SharedUserInterfaces.MediumText("Statuses");
            ImGui.PushTextWrapPos(ImGui.GetContentRegionAvail().X);
            ImGui.TextWrapped("Various aspects of the plugin will modify or control your character. You can find any that are currently active in the list below.");
            ImGui.PopTextWrapPos();
        });
        
        // Count the number of statuses that have been added, useful for displaying "No Statuses" at the end if nothing was there.
        var count = 0;
        
        // The size of the button beside the status
        var buttonSize = new Vector2(ImGui.GetFontSize() * 2 + AetherRemoteImGui.WindowPadding.Y + AetherRemoteImGui.ItemSpacing.Y);
        
        if (statusManager.CustomizePlus is { } customizePlus)
        {
            count++;
            SharedUserInterfaces.ContentBox("StatusCustomizePlus", AetherRemoteColors.PanelColor, true, () =>
            {
                if (SharedUserInterfaces.IconButton(FontAwesomeIcon.Plus, buttonSize, "Click to dispel", "CustomizePlusStatusButton"))
                    _ = controller.ClearCustomizePlus().ConfigureAwait(false);
                
                ImGui.SameLine();
                ImGui.BeginGroup();
                ImGui.Text("Customize+");
                ImGui.Text($"{customizePlus.Applier.NoteOrFriendCode} applied a C+ profile to you.");
                ImGui.EndGroup();
            });
        }
        
        if (statusManager.GlamourerPenumbra is { } glamourerPenumbra)
        {
            count++;
            SharedUserInterfaces.ContentBox("StatusGlamourerPenumbra", AetherRemoteColors.PanelColor, true, () =>
            {
                if (SharedUserInterfaces.IconButton(FontAwesomeIcon.WandMagicSparkles, buttonSize, "Click to dispel", "CustomizePlusStatusButton"))
                    _ = controller.ClearGlamourerPenumbra().ConfigureAwait(false);
                
                ImGui.SameLine();
                ImGui.BeginGroup();
                ImGui.Text("Glamourer & Penumbra");
                ImGui.Text($"{glamourerPenumbra.Applier.NoteOrFriendCode} changed your appearance or modified your collection (via body swap or twinning).");
                ImGui.EndGroup();
            });
        }
        
        if (statusManager.Honorific is { } honorific)
        {
            count++;
            SharedUserInterfaces.ContentBox("StatusHonorific", AetherRemoteColors.PanelColor, true, () =>
            {
                if (SharedUserInterfaces.IconButton(FontAwesomeIcon.Crown, buttonSize, "Click to dispel", "CustomizePlusStatusButton"))
                    controller.ClearHonorific();

                ImGui.SameLine();
                ImGui.BeginGroup();
                ImGui.Text("Honorific");
                ImGui.Text($"{honorific.Applier.NoteOrFriendCode} applied an honorific to you.");
                ImGui.EndGroup();
            });
        }
        
        if (statusManager.Hypnosis is { } hypnosis)
        {
            count++;
            SharedUserInterfaces.ContentBox("StatusHypnosis", AetherRemoteColors.PanelColor, true, () =>
            {
                if (SharedUserInterfaces.IconButton(FontAwesomeIcon.Stopwatch, buttonSize, "Click to dispel", "CustomizePlusStatusButton"))
                    controller.ClearHypnosis();

                ImGui.SameLine();
                ImGui.BeginGroup();
                ImGui.Text("Hypnosis");
                ImGui.Text($"{hypnosis.Applier.NoteOrFriendCode} began hypnotizing you.");
                ImGui.EndGroup();
            });
        }
        
        if (statusManager.Possession is { } possession)
        {
            count++;
            SharedUserInterfaces.ContentBox("StatusPossession", AetherRemoteColors.PanelColor, true, () =>
            {
                if (SharedUserInterfaces.IconButton(FontAwesomeIcon.Ghost, buttonSize, "Click to dispel", "CustomizePlusStatusButton"))
                    _ = controller.ClearPossession().ConfigureAwait(false);

                ImGui.SameLine();
                ImGui.BeginGroup();
                ImGui.Text("Customize+");
                ImGui.Text($"{possession.Applier.NoteOrFriendCode} is possessing you.");
                ImGui.EndGroup();
            });
        }

        // If you haven't had any statuses applied
        if (count is 0)
        {
            SharedUserInterfaces.ContentBox("StatusNoStatuses", AetherRemoteColors.PanelColor, true, () =>
            {
                ImGui.TextWrapped("You do not have any statuses affecting you.");
            });
        }
        
        ImGui.EndChild();
    }
}
