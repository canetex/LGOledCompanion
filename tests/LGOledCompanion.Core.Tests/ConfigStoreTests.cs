using LGOledCompanion.Core;

namespace LGOledCompanion.Core.Tests;

public sealed class ConfigStoreTests
{
    [Fact]
    public void Missing_file_returns_defaults_and_does_not_create_it()
    {
        var path = Path.Combine(Path.GetTempPath(), $"lgoc-{Guid.NewGuid():N}.json");
        var store = new ConfigStore(path);

        var config = store.Load();

        Assert.False(File.Exists(path));
        Assert.Equal(5, config.Tempo_Inatividade);
        Assert.Equal(10, config.Tempo_FadeOut_Overlay);
        Assert.Equal(10, config.Tempo_FadeIn_Overlay);
        Assert.Equal(30, config.Tempo_Transicao);
        Assert.Equal(0, config.Opacidade_Overlay);
        Assert.Equal("#000000", config.Cor_Overlay);
        Assert.Equal("#FF3B7C", config.Cor_Destaque);
        Assert.Equal(string.Empty, config.Pasta_Fotos);
        Assert.Equal("23:00", config.Horario_Noite_Inicio);
        Assert.Equal("07:00", config.Horario_Noite_Fim);
        Assert.Equal("Device1", config.Device_WebOS);
        Assert.Equal(@"C:\Program Files\LGTV Companion\LGTVcli.exe", config.Caminho_LGTVcli);
        Assert.False(config.Modo_Debug);
    }

    [Fact]
    public void Save_writes_file_that_load_can_read()
    {
        var path = Path.Combine(Path.GetTempPath(), $"lgoc-{Guid.NewGuid():N}.json");
        try
        {
            var store = new ConfigStore(path);
            var config = store.Load();
            config.Pasta_Fotos = @"D:\Fotos";
            config.Tempo_Inatividade = 8;
            config.Tempo_FadeOut_Overlay = 12;
            config.Tempo_FadeIn_Overlay = 8;
            config.Tempo_Transicao = 25;
            config.Cor_Destaque = "#22D3EE";
            store.Save(config);

            var loaded = new ConfigStore(path).Load();
            Assert.True(File.Exists(path));
            Assert.Equal(@"D:\Fotos", loaded.Pasta_Fotos);
            Assert.Equal(8, loaded.Tempo_Inatividade);
            Assert.Equal(12, loaded.Tempo_FadeOut_Overlay);
            Assert.Equal(8, loaded.Tempo_FadeIn_Overlay);
            Assert.Equal(25, loaded.Tempo_Transicao);
            Assert.Equal("#22D3EE", loaded.Cor_Destaque);
        }
        finally
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }
    }
}
