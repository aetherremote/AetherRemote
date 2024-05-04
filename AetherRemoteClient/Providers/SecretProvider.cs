using AetherRemoteClient.Domain;
using AetherRemoteClient.Domain.Saves;
using Dalamud.Plugin;

namespace AetherRemoteClient.Providers;

public class SecretProvider(DalamudPluginInterface pluginInterface)
{
    private const string FileName = "secret.json";
    private readonly SaveFile<SecretSave> saveSystem = new(pluginInterface.ConfigDirectory.FullName, FileName);

    public string Secret
    {
        get
        {
            return saveSystem.Get.Secret;
        }
        set
        {
            saveSystem.Get.Secret = value;
        }
    }

    public void Save()
    {
        saveSystem.Save();
    }
}
