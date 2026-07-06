using System.Numerics;
using AetherRemoteClient.Domain.Enums;
using AetherRemoteClient.UI.Style;
using AetherRemoteClient.Utils;
using Dalamud.Bindings.ImGui;

namespace AetherRemoteClient.UI.Views.Login;

public partial class LoginView
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

            var preview = _activeSessionService.PendingSecretId is not { } secretId
                ? "Select a secret to log in with"
                : _configurationService.Secrets.TryGetValue(secretId, out var value)
                    ? value.Name
                    : "<<Unable to find secret>>";
            
            if (ImGui.BeginCombo("##SecretSelect", preview))
            {
                foreach (var secret in _configurationService.Secrets)
                    if (ImGui.Selectable(secret.Value.Name))
                        _ = _activeSessionService.UpdatePendingSecretId(secret.Key).ConfigureAwait(false);
                
                ImGui.EndCombo();
            }
            
            ImGui.SameLine();

            var disable = _networkService.State is not ConnectionState.Disconnected;
            if (disable) ImGui.BeginDisabled();
            if (ImGui.Button("Connect"))
                _ = Connect().ConfigureAwait(false);
            if (disable) ImGui.EndDisabled();
            
            ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, new Vector2(4, 0));
            ImGui.TextUnformatted("Need a secret? Join the");
            ImGui.SameLine();
            ImGui.PushStyleColor(ImGuiCol.Text, AetherRemoteColors.DiscordBlue);
            var size = ImGui.CalcTextSize("discord");
            if (ImGui.Selectable("discord", false, ImGuiSelectableFlags.None, size))
                OpenDiscordLink();
            ImGui.PopStyleColor();
            ImGui.SameLine();
            ImGui.TextUnformatted("to generate one, then add it in the settings tab.");
            ImGui.PopStyleVar();
        });

        ImGui.EndChild();
    }
}