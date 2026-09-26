using LGOledCompanion.Core;

namespace LGOledCompanion.Core.Tests;

public sealed class OverlayTintTests
{
    [Fact]
    public void Parses_rrggbb_hex()
    {
        var tint = OverlayTint.Parse("#1A2B3C");
        Assert.Equal(0x1A, tint.R);
        Assert.Equal(0x2B, tint.G);
        Assert.Equal(0x3C, tint.B);
        Assert.Equal("#1A2B3C", tint.ToHex());
    }

    [Fact]
    public void Parses_short_hex()
    {
        var tint = OverlayTint.Parse("#F80");
        Assert.Equal(0xFF, tint.R);
        Assert.Equal(0x88, tint.G);
        Assert.Equal(0x00, tint.B);
    }

    [Fact]
    public void Invalid_hex_falls_back_to_black()
    {
        var tint = OverlayTint.Parse("not-a-color");
        Assert.Equal(0, tint.R);
        Assert.Equal(0, tint.G);
        Assert.Equal(0, tint.B);
        Assert.Equal("#000000", tint.ToHex());
    }

    [Fact]
    public void Config_default_overlay_color_is_black()
    {
        Assert.Equal("#000000", new AppConfig().Cor_Overlay);
    }
}
