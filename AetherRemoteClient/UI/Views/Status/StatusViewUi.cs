using System;
using System.Numerics;
using AetherRemoteClient.Domain;
using AetherRemoteClient.Domain.Interfaces;
using AetherRemoteClient.Ipc;
using AetherRemoteClient.Services;
using AetherRemoteClient.UI.Components.Input;
using AetherRemoteClient.Utils;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
using Dalamud.Interface.Colors;
using Newtonsoft.Json.Linq;

namespace AetherRemoteClient.UI.Views.Status;

public class StatusViewUi(
    GlamourerIpc glamourer,
    StatusViewUiController controller,
    IdentityService identityService,
    PermanentLockService permanentLockService,
    TipService tipService,
    SpiralService spiralService) : IDrawable
{
    public void Draw()
    {
        ImGui.BeginChild("SettingsContent", Vector2.Zero, false, AetherRemoteStyle.ContentFlags);

        if (ImGui.Button("A"))
        {
            PrivateA();
        }
        ImGui.SameLine();
        if (ImGui.Button("B"))
        {
            PrivateB();
        }
        
        var windowWidth = ImGui.GetWindowWidth();
        var windowPadding = ImGui.GetStyle().WindowPadding;

        ImGui.AlignTextToFramePadding();

        SharedUserInterfaces.ContentBox("StatusHeader", AetherRemoteStyle.PanelBackground, true, () =>
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
        
        SharedUserInterfaces.ContentBox("StatusLogout", AetherRemoteStyle.PanelBackground, true, () =>
        {
            SharedUserInterfaces.MediumText("Welcome");
            ImGui.TextUnformatted(tipService.CurrentTip);
        });

        if (SharedUserInterfaces.ContextBoxButton(FontAwesomeIcon.Plug, windowPadding, windowWidth))
            controller.Disconnect();
        
        SharedUserInterfaces.Tooltip("Disconnect");
        
        SharedUserInterfaces.ContentBox("StatusButtons", AetherRemoteStyle.PanelBackground, true, () =>
        {
            SharedUserInterfaces.MediumText("Statuses");
            ImGui.TextUnformatted("Various aspects of the plugin have lingering affects. You can find them below.");
            
            ImGui.SameLine();
            SharedUserInterfaces.Icon(FontAwesomeIcon.QuestionCircle);
            SharedUserInterfaces.Tooltip(
                [
                    "Only active statuses will be displayed. Such statuses include:",
                    "- Being permanently transformed",
                    "- Being transformed",
                    "- Being body swapped",
                    "- Being twinned",
                    "- Being hypnotized"
                ]);
        });
        
        if (permanentLockService.IsLocked)
            RenderPermanentTransformationComponent(windowPadding, windowWidth);

        if (identityService.IsAltered)
            RenderTransformationComponent(windowPadding, windowWidth);

        if (spiralService.IsBeingHypnotized)
            RenderHypnosisComponent(windowPadding, windowWidth);
        
        ImGui.EndChild();
    }
    
    private void RenderPermanentTransformationComponent(Vector2 windowPadding, float windowWidth)
    {
        SharedUserInterfaces.ContentBox("StatusLock", AetherRemoteStyle.ElevatedBackground, true, () =>
        {
            SharedUserInterfaces.MediumText("Permanently Transformed");
            ImGui.TextUnformatted($"{identityService.Alteration?.Sender ?? "Unknown"} has locked your appearance");
        });

        var previousContextBoxSize = ImGui.GetItemRectSize();
        var endingCursorPosition = ImGui.GetCursorPosY();
        
        SharedUserInterfaces.PushBigFont();
        var cursorPositionStartX = windowWidth - previousContextBoxSize.Y - FourDigitInput.Width - windowPadding.Y * 2;
        var start = new Vector2(cursorPositionStartX ,endingCursorPosition - previousContextBoxSize.Y - windowPadding.Y * 2);
        ImGui.SetCursorPos(start);
        
        controller.PinInput.Draw();
        SharedUserInterfaces.PopBigFont();
        
        ImGui.SameLine();

        if (SharedUserInterfaces.IconButton(FontAwesomeIcon.Unlock, new Vector2(previousContextBoxSize.Y)))
            controller.Unlock();
        
        ImGui.SetCursorPosY(endingCursorPosition);
    }

    private void RenderTransformationComponent(Vector2 windowPadding, float windowWidth)
    {
        SharedUserInterfaces.ContentBox("StatusTransformation", AetherRemoteStyle.PanelBackground, true, () =>
        {
            SharedUserInterfaces.MediumText("Identity Altered");

            if (identityService.Alteration is not { } alteration)
            {
                ImGui.TextUnformatted("An unknown friend altered your identity");
                return;
            }

            var type = alteration.Type switch
            {
                IdentityAlterationType.Transformation => $"{alteration.Sender} transformed you or your clothing",
                IdentityAlterationType.Twinning => $"{alteration.Sender} twinned with you",
                IdentityAlterationType.BodySwap => $"{alteration.Sender} swapped your body",
                _ => $"{alteration.Sender} altered your identity"
            };
            
            ImGui.TextUnformatted(type);
        });

        if (permanentLockService.IsLocked)
        {
            ImGui.BeginDisabled();
            SharedUserInterfaces.ContextBoxButton(FontAwesomeIcon.History, windowPadding, windowWidth);
            ImGui.EndDisabled();
        }
        else
        {
            if (SharedUserInterfaces.ContextBoxButton(FontAwesomeIcon.History, windowPadding, windowWidth))
                controller.ResetIdentity();
        }
    }

    private void RenderHypnosisComponent(Vector2 windowPadding, float windowWidth)
    {
        SharedUserInterfaces.ContentBox("StatusHypnosis", AetherRemoteStyle.PanelBackground, true, () =>
        {
            SharedUserInterfaces.MediumText("Hypnosis");
            ImGui.TextUnformatted($"{identityService.Alteration?.Sender ?? "Unknown"} is hypnotizing you");
        });
        
        if (SharedUserInterfaces.ContextBoxButton(FontAwesomeIcon.Square, windowPadding, windowWidth))
            spiralService.StopCurrentSpiral();
    }

    private async void PrivateA()
    {
        try
        {
            var g = await glamourer.GetDesignAsync();
            var k = GlamourerIpc.ConvertGlamourerBase64StringToJObject(g);
            
            var t = await glamourer.GetDesignComponentsAsync();
            ImGui.SetClipboardText(t.ToString());
        }
        catch (Exception e)
        {
            Plugin.Log.Info($"{e}");
        }
    }

    private async void PrivateB()
    {
        try
        {
           
        }
        catch (Exception e)
        {
            Plugin.Log.Info($"{e}");
        }
    }
    
    
    
    public void MergeMissingMaterialsWithRevert(JObject oldObj, JObject newObj)
    {
        var oldMaterials = oldObj["Materials"] as JObject;
        var newMaterials = newObj["Materials"] as JObject;

        if (oldMaterials == null || newMaterials == null)
            return;

        foreach (var oldMat in oldMaterials.Properties())
        {
            if (!newMaterials.ContainsKey(oldMat.Name))
            {
                // Deep clone the material to avoid reference issues
                var newMat = (JObject)oldMat.Value.DeepClone();

                // Set Revert = true
                newMat["Revert"] = true;

                // Add to new materials
                newMaterials[oldMat.Name] = newMat;
            }
        }
    }
}
