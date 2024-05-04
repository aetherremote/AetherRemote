using System;
using System.IO;
using System.Text.Json;

namespace AetherRemoteClient.Domain;

public class SaveFile<T>
{
    private static readonly JsonSerializerOptions SerializationOptions = new() { WriteIndented = true };
    private string filePath { get; init; }

    public T Get => save;
    private readonly T save;

    public SaveFile(string fileDirectory, string fileName)
    {
        filePath = Path.Combine(fileDirectory, fileName);
        save = Load();
    }

    protected T Load()
    {
        T? loaded;
        if (!File.Exists(filePath))
        {
            loaded = (T)Activator.CreateInstance(typeof(T))!;
            Save();
        }
        else
        {
            try
            {
                loaded = JsonSerializer.Deserialize<T>(File.ReadAllText(filePath));
            }
            catch
            {
                loaded = default;
            }

            if (loaded == null)
            {
                loaded = (T)Activator.CreateInstance(typeof(T))!;
                Save();
            }
        }
        
        return loaded;
    }

    public async void Save()
    {
        try
        {
            await File.WriteAllTextAsync(filePath, JsonSerializer.Serialize(save, SerializationOptions));
        }
        catch { }
    }
}
