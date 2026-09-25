using LGOledCompanion.Core;

namespace LGOledCompanion.Core.Tests;

public sealed class SessionPolicyTests
{
    private static SessionSnapshot DesktopIdleDay() => new()
    {
        State = SessionState.Desktop,
        IdleFor = TimeSpan.FromMinutes(5),
        IdleThreshold = TimeSpan.FromMinutes(5),
        IsNight = false
    };

    [Fact]
    public void Idle_day_starts_slideshow()
    {
        Assert.Equal(SessionAction.StartSlideshow, SessionPolicy.Decide(DesktopIdleDay()));
    }

    [Fact]
    public void Idle_night_turns_screen_off()
    {
        var snapshot = DesktopIdleDay() with { IsNight = true };
        Assert.Equal(SessionAction.ScreenOff, SessionPolicy.Decide(snapshot));
    }

    [Fact]
    public void Below_threshold_stays_idle()
    {
        var snapshot = DesktopIdleDay() with { IdleFor = TimeSpan.FromMinutes(4) };
        Assert.Equal(SessionAction.None, SessionPolicy.Decide(snapshot));
    }

    [Theory]
    [InlineData(true, false, false)]
    [InlineData(false, true, false)]
    [InlineData(false, false, true)]
    public void Blockers_prevent_idle_action(bool paused, bool settings_open, bool display_required)
    {
        var snapshot = DesktopIdleDay() with
        {
            IsPaused = paused,
            SettingsOpen = settings_open,
            DisplayRequired = display_required
        };
        Assert.Equal(SessionAction.None, SessionPolicy.Decide(snapshot));
    }

    [Fact]
    public void Input_during_slideshow_stops_it()
    {
        var snapshot = DesktopIdleDay() with
        {
            State = SessionState.Slideshow,
            IdleFor = TimeSpan.FromSeconds(1)
        };
        Assert.Equal(SessionAction.StopSlideshow, SessionPolicy.Decide(snapshot));
    }

    [Fact]
    public void Display_required_during_slideshow_stops_it()
    {
        var snapshot = DesktopIdleDay() with
        {
            State = SessionState.Slideshow,
            DisplayRequired = true
        };
        Assert.Equal(SessionAction.StopSlideshow, SessionPolicy.Decide(snapshot));
    }

    [Fact]
    public void Crossing_into_night_while_slideshow_switches_off()
    {
        var snapshot = DesktopIdleDay() with
        {
            State = SessionState.Slideshow,
            IsNight = true
        };
        Assert.Equal(SessionAction.SwitchToNight, SessionPolicy.Decide(snapshot));
    }

    [Fact]
    public void Input_during_night_turns_screen_on()
    {
        var snapshot = DesktopIdleDay() with
        {
            State = SessionState.NightScreenOff,
            IsNight = true,
            IdleFor = TimeSpan.FromSeconds(1)
        };
        Assert.Equal(SessionAction.ScreenOn, SessionPolicy.Decide(snapshot));
    }

    [Fact]
    public void Crossing_into_day_while_still_idle_starts_slideshow()
    {
        var snapshot = DesktopIdleDay() with
        {
            State = SessionState.NightScreenOff,
            IsNight = false
        };
        Assert.Equal(SessionAction.SwitchToDay, SessionPolicy.Decide(snapshot));
    }

    [Fact]
    public void Display_required_during_night_turns_screen_on()
    {
        var snapshot = DesktopIdleDay() with
        {
            State = SessionState.NightScreenOff,
            IsNight = true,
            DisplayRequired = true
        };
        Assert.Equal(SessionAction.ScreenOn, SessionPolicy.Decide(snapshot));
    }

    [Fact]
    public void Stays_in_slideshow_when_still_idle_by_day()
    {
        var snapshot = DesktopIdleDay() with { State = SessionState.Slideshow };
        Assert.Equal(SessionAction.None, SessionPolicy.Decide(snapshot));
    }
}
