using AetherRemoteClient.Domain.UI;
using Dalamud.Interface;
using ImGuiNET;
using System;

namespace AetherRemoteClient.UI.Tabs.Modules;

public interface IControlTableModule : IDisposable
{
    public void Draw();
}
