// OverlayCycle.cs
// Top 5: At O(1), Total O(1), Lerp O(1)

namespace LGOledCompanion.Core;

public readonly record struct OverlayCycleProgress(double Opacity, bool Completed);

public static class OverlayCycle
{
    public static readonly TimeSpan MaxDuration = TimeSpan.FromSeconds(120);

    public static TimeSpan Total(TimeSpan fade_out, TimeSpan hold, TimeSpan fade_in)
    {
        var out_time = Clamp(fade_out);
        var hold_time = Clamp(hold);
        var in_time = Clamp(fade_in);
        var total = out_time + hold_time + in_time;
        return total > TimeSpan.Zero ? total : TimeSpan.FromSeconds(1);
    }

    public static OverlayCycleProgress At(
        TimeSpan elapsed,
        TimeSpan fade_out,
        TimeSpan hold,
        TimeSpan fade_in,
        double configured_opacity)
    {
        var configured = Math.Clamp(configured_opacity, 0, 0.9);
        var out_time = Clamp(fade_out);
        var hold_time = Clamp(hold);
        var in_time = Clamp(fade_in);
        var total = Total(out_time, hold_time, in_time);

        if (elapsed >= total)
        {
            return new OverlayCycleProgress(configured, true);
        }

        if (elapsed <= out_time)
        {
            var amount = out_time == TimeSpan.Zero ? 1 : elapsed / out_time;
            return new OverlayCycleProgress(Lerp(1.0, configured, amount), false);
        }

        if (elapsed < out_time + hold_time)
        {
            return new OverlayCycleProgress(0, false);
        }

        var into_fade_in = elapsed - out_time - hold_time;
        var fade_amount = in_time == TimeSpan.Zero ? 1 : into_fade_in / in_time;
        return new OverlayCycleProgress(Lerp(0, configured, fade_amount), false);
    }

    private static TimeSpan Clamp(TimeSpan value)
    {
        if (value < TimeSpan.Zero)
        {
            return TimeSpan.Zero;
        }

        return value > MaxDuration ? MaxDuration : value;
    }

    private static double Lerp(double from, double to, double amount)
    {
        return from + (to - from) * Math.Clamp(amount, 0, 1);
    }
}
