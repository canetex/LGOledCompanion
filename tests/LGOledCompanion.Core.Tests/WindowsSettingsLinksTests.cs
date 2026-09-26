using LGOledCompanion.Core;

namespace LGOledCompanion.Core.Tests;

public sealed class WindowsSettingsLinksTests
{
    [Fact]
    public void Screensaver_opens_lock_screen_settings()
    {
        Assert.Equal("ms-settings:lockscreen", WindowsSettingsLinks.Screensaver);
    }

    [Fact]
    public void Display_off_opens_power_and_sleep_settings()
    {
        Assert.Equal("ms-settings:powersleep", WindowsSettingsLinks.DisplayOff);
    }
}
