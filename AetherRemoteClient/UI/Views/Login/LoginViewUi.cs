using System.Numerics;
using AetherRemoteClient.Domain.Enums;
using AetherRemoteClient.Domain.Interfaces;
using AetherRemoteClient.Services;
using AetherRemoteClient.UI.Style;
using AetherRemoteClient.Utils;
using Dalamud.Bindings.ImGui;

namespace AetherRemoteClient.UI.Views.Login;

public class LoginViewUi(LoginViewUiController controller, NetworkService networkService, SecretsService secretsService) : IDrawable
{
    public void Draw()
    {
        ImGui.BeginChild("LoginContent", Vector2.Zero, false, AetherRemoteImGui.ContentFlags);

        ImGui.AlignTextToFramePadding();

        SharedUserInterfaces.ContentBox("LoginHeader", AetherRemoteColors.PanelColor, true, () =>
        {
            SharedUserInterfaces.BigTextCentered("Aether Remote");
            SharedUserInterfaces.TextCentered(Plugin.Version.ToString());
        });
        
        SharedUserInterfaces.ContentBox("LoginSecretSelect", AetherRemoteColors.PanelColor, true, () =>
        {
            SharedUserInterfaces.MediumText("Login with Secret");

            var preview = controller.GetCurrentSecret()?.Name ?? "Select secret...";
            if (ImGui.BeginCombo("##SecretSelect", preview))
            {
                foreach (var secret in secretsService.Secrets)
                    if (ImGui.Selectable(secret.Value.Name))
                        _ = controller.SetSecret(secret.Value).ConfigureAwait(false);
                
                ImGui.EndCombo();
            }
            
            ImGui.SameLine();

            var disable = networkService.State is not ConnectionState.Disconnected;
            if (disable) ImGui.BeginDisabled();
            if (ImGui.Button("Connect"))
                _ = controller.Connect().ConfigureAwait(false);
            if (disable) ImGui.EndDisabled();
            
            ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, new Vector2(4, 0));
            ImGui.TextUnformatted("Need a secret? Join the");
            ImGui.SameLine();
            ImGui.PushStyleColor(ImGuiCol.Text, AetherRemoteColors.DiscordBlue);
            var size = ImGui.CalcTextSize("discord");
            if (ImGui.Selectable("discord", false, ImGuiSelectableFlags.None, size))
                LoginViewUiController.OpenDiscordLink();
            ImGui.PopStyleColor();
            ImGui.SameLine();
            ImGui.TextUnformatted("to generate one, then add it in the settings tab.");
            ImGui.PopStyleVar();
        });

        ImGui.EndChild();
    }
}