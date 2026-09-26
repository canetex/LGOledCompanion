// ConfigStore.cs
// Top 5: Load O(bytes), Save O(1), Clamp O(1)

using System.Text.Json;

namespace LGOledCompanion.Core;

public sealed class ConfigStore
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true
    };

    private readonly string _path;

    public ConfigStore(string path)
    {
        _path = path;
    }

    public AppConfig Load()
    {
        if (!File.Exists(_path))
        {
            return new AppConfig();
        }

        try
        {
            var json = File.ReadAllText(_path);
            return JsonSerializer.Deserialize<AppConfig>(json) ?? new AppConfig();
        }
        catch
        {
            return new AppConfig();
        }
    }

    public void Save(AppConfig config)
    {
        if (config.Tempo_Inatividade < 1)
        {
            config.Tempo_Inatividade = 5;
        }

        config.Opacidade_Overlay = Math.Clamp(config.Opacidade_Overlay, 0, 90);
        config.Tempo_FadeOut_Overlay = Math.Clamp(config.Tempo_FadeOut_Overlay, 0, 120);
        config.Tempo_FadeIn_Overlay = Math.Clamp(config.Tempo_FadeIn_Overlay, 0, 120);
        config.Tempo_Transicao = Math.Clamp(config.Tempo_Transicao, 0, 120);

        var json = JsonSerializer.Serialize(config, JsonOptions);
        File.WriteAllText(_path, json);
    }
}
