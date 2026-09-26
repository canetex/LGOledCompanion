using LGOledCompanion.Core;

namespace LGOledCompanion.Core.Tests;

public sealed class ScreenPlacementTests
{
    [Fact]
    public void Scale_1_keeps_pixel_rectangle()
    {
        var dip = ScreenPlacement.ToDip(0, 0, 4096, 2160, 1, 1);
        Assert.Equal(0, dip.Left);
        Assert.Equal(0, dip.Top);
        Assert.Equal(4096, dip.Width);
        Assert.Equal(2160, dip.Height);
    }

    [Fact]
    public void Scale_2_converts_4k_bounds_to_dip()
    {
        var dip = ScreenPlacement.ToDip(0, 0, 4096, 2160, 2, 2);
        Assert.Equal(0, dip.Left);
        Assert.Equal(0, dip.Top);
        Assert.Equal(2048, dip.Width);
        Assert.Equal(1080, dip.Height);
    }

    [Fact]
    public void Offset_secondary_monitor_is_scaled()
    {
        var dip = ScreenPlacement.ToDip(4096, 0, 2560, 1440, 1.25, 1.25);
        Assert.Equal(3276.8, dip.Left, 5);
        Assert.Equal(0, dip.Top);
        Assert.Equal(2048, dip.Width);
        Assert.Equal(1152, dip.Height);
    }
}
