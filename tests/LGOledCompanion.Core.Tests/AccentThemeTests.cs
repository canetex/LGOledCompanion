using LGOledCompanion.Core;

namespace LGOledCompanion.Core.Tests;

public sealed class AccentThemeTests
{
    [Fact]
    public void Config_default_accent_is_magenta()
    {
        Assert.Equal("#FF3B7C", new AppConfig().Cor_Destaque);
    }

    [Fact]
    public void Unknown_hex_falls_back_to_magenta()
    {
        Assert.Equal("#FF3B7C", AccentTheme.Normalize("not-a-color"));
        Assert.Equal("#FF3B7C", AccentTheme.Normalize("#123456"));
    }

    [Fact]
    public void Known_presets_keep_their_hex()
    {
        Assert.Equal("#FF3B7C", AccentTheme.Normalize("#ff3b7c"));
        Assert.Equal("#22D3EE", AccentTheme.Normalize("#22D3EE"));
        Assert.Equal("#84CC16", AccentTheme.Normalize("#84CC16"));
        Assert.Equal("#F97316", AccentTheme.Normalize("#F97316"));
    }

    [Fact]
    public void Index_matches_combo_order()
    {
        Assert.Equal(0, AccentTheme.IndexOf("#FF3B7C"));
        Assert.Equal(1, AccentTheme.IndexOf("#22D3EE"));
        Assert.Equal(2, AccentTheme.IndexOf("#84CC16"));
        Assert.Equal(3, AccentTheme.IndexOf("#F97316"));
        Assert.Equal(0, AccentTheme.IndexOf("nope"));
    }
}
