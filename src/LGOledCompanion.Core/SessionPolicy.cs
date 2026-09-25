// SessionPolicy.cs
// Top 5: Decide O(1)

namespace LGOledCompanion.Core;

public enum SessionState
{
    Desktop,
    Slideshow,
    NightScreenOff
}

public enum SessionAction
{
    None,
    StartSlideshow,
    StopSlideshow,
    ScreenOff,
    ScreenOn,
    SwitchToNight,
    SwitchToDay
}

public sealed record SessionSnapshot
{
    public SessionState State { get; init; }
    public bool IsPaused { get; init; }
    public bool SettingsOpen { get; init; }
    public bool DisplayRequired { get; init; }
    public TimeSpan IdleFor { get; init; }
    public TimeSpan IdleThreshold { get; init; }
    public bool IsNight { get; init; }
}

public static class SessionPolicy
{
    public static SessionAction Decide(SessionSnapshot snapshot)
    {
        var blocked = snapshot.IsPaused || snapshot.SettingsOpen || snapshot.DisplayRequired;
        var idle = snapshot.IdleFor >= snapshot.IdleThreshold;

        return snapshot.State switch
        {
            SessionState.Slideshow => DecideSlideshow(blocked, idle, snapshot.IsNight),
            SessionState.NightScreenOff => DecideNight(blocked, idle, snapshot.IsNight),
            _ => DecideDesktop(blocked, idle, snapshot.IsNight)
        };
    }

    private static SessionAction DecideSlideshow(bool blocked, bool idle, bool is_night)
    {
        if (blocked || !idle)
        {
            return SessionAction.StopSlideshow;
        }

        return is_night ? SessionAction.SwitchToNight : SessionAction.None;
    }

    private static SessionAction DecideNight(bool blocked, bool idle, bool is_night)
    {
        if (blocked || !idle)
        {
            return SessionAction.ScreenOn;
        }

        return is_night ? SessionAction.None : SessionAction.SwitchToDay;
    }

    private static SessionAction DecideDesktop(bool blocked, bool idle, bool is_night)
    {
        if (blocked || !idle)
        {
            return SessionAction.None;
        }

        return is_night ? SessionAction.ScreenOff : SessionAction.StartSlideshow;
    }
}
