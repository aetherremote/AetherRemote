using System.Numerics;
using AetherRemoteClient.Domain.Interfaces;
using AetherRemoteClient.Services;
using AetherRemoteClient.Utils;
using ImGuiNET;
using Microsoft.AspNetCore.SignalR.Client;

namespace AetherRemoteClient.UI.Views.Login;

public class LoginViewUi(NetworkService networkService) : IDrawable
{
    private readonly LoginViewUiController _controller = new(networkService);
    
    public bool Draw()
    {
        ImGui.BeginChild("OverridesContent", Vector2.Zero, false, AetherRemoteStyle.ContentFlags);
        
        ImGui.AlignTextToFramePadding();
        
        SharedUserInterfaces.ContentBox(AetherRemoteStyle.PanelBackground, () =>
        {
            SharedUserInterfaces.BigTextCentered("Aether Remote");
            SharedUserInterfaces.TextCentered(Plugin.Version.ToString());
        });
        
        SharedUserInterfaces.ContentBox(AetherRemoteStyle.PanelBackground, () =>
        {
            var shouldConnect = false;
            
            SharedUserInterfaces.MediumText("Enter Secret");
            if (ImGui.InputTextWithHint("##SecretInput", "Secret", ref _controller.Secret, 100, ImGuiInputTextFlags.EnterReturnsTrue))
                shouldConnect = true;
            
            ImGui.SameLine();
            var disable = networkService.Connection.State is not HubConnectionState.Disconnected;
            
            if (disable)
                ImGui.BeginDisabled();
            
            if (ImGui.Button("Connect"))
                shouldConnect = true;
            
            if (disable)
                ImGui.EndDisabled();
            
            if (shouldConnect)
                _controller.Connect();
            
            ImGui.Spacing();
            
            ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, new Vector2(4, 0));
            
            ImGui.TextUnformatted("Need a secret? Join the");
            ImGui.SameLine();
            ImGui.PushStyleColor(ImGuiCol.Text, AetherRemoteStyle.DiscordBlue);
            var size = ImGui.CalcTextSize("discord");
            if (ImGui.Selectable("discord", false, ImGuiSelectableFlags.None, size))
                LoginViewUiController.OpenDiscordLink();
            
            ImGui.PopStyleColor();
            ImGui.SameLine();
            ImGui.TextUnformatted("to generate one.");
            
            ImGui.PopStyleVar();
        });
        
        ImGui.EndChild();
        return false;
    }
}