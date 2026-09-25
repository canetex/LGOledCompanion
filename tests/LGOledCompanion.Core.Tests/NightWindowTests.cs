using LGOledCompanion.Core;

namespace LGOledCompanion.Core.Tests;

public sealed class NightWindowTests
{
    [Fact]
    public void Same_start_and_end_disables_night()
    {
        var window = NightWindow.Parse("23:00", "23:00");
        Assert.False(window.IsActive(new TimeOnly(23, 0)));
        Assert.False(window.IsActive(new TimeOnly(3, 0)));
    }

    [Fact]
    public void Same_day_range_is_half_open()
    {
        var window = NightWindow.Parse("01:00", "07:00");
        Assert.False(window.IsActive(new TimeOnly(0, 59)));
        Assert.True(window.IsActive(new TimeOnly(1, 0)));
        Assert.True(window.IsActive(new TimeOnly(6, 59)));
        Assert.False(window.IsActive(new TimeOnly(7, 0)));
    }

    [Fact]
    public void Range_crossing_midnight_includes_evening_and_dawn()
    {
        var window = NightWindow.Parse("23:00", "07:00");
        Assert.False(window.IsActive(new TimeOnly(22, 59)));
        Assert.True(window.IsActive(new TimeOnly(23, 0)));
        Assert.True(window.IsActive(new TimeOnly(3, 30)));
        Assert.True(window.IsActive(new TimeOnly(6, 59)));
        Assert.False(window.IsActive(new TimeOnly(7, 0)));
    }
}
