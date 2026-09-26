using LGOledCompanion.Core;

namespace LGOledCompanion.Core.Tests;

public sealed class OverlayCycleTests
{
    private static readonly TimeSpan FadeOut = TimeSpan.FromSeconds(10);
    private static readonly TimeSpan Hold = TimeSpan.FromSeconds(30);
    private static readonly TimeSpan FadeIn = TimeSpan.FromSeconds(10);
    private const double Configured = 0.3;

    [Fact]
    public void Starts_fully_covered()
    {
        var progress = OverlayCycle.At(TimeSpan.Zero, FadeOut, Hold, FadeIn, Configured);
        Assert.Equal(1.0, progress.Opacity);
        Assert.False(progress.Completed);
    }

    [Fact]
    public void Mid_fade_out_lerps_toward_configured()
    {
        var progress = OverlayCycle.At(TimeSpan.FromSeconds(5), FadeOut, Hold, FadeIn, Configured);
        Assert.Equal(0.65, progress.Opacity, 5);
        Assert.False(progress.Completed);
    }

    [Fact]
    public void End_of_fade_out_is_configured()
    {
        var progress = OverlayCycle.At(TimeSpan.FromSeconds(10), FadeOut, Hold, FadeIn, Configured);
        Assert.Equal(0.3, progress.Opacity, 5);
        Assert.False(progress.Completed);
    }

    [Fact]
    public void Hold_phase_is_clear()
    {
        var progress = OverlayCycle.At(TimeSpan.FromSeconds(25), FadeOut, Hold, FadeIn, Configured);
        Assert.Equal(0.0, progress.Opacity);
        Assert.False(progress.Completed);
    }

    [Fact]
    public void Mid_fade_in_lerps_from_clear_to_configured()
    {
        var progress = OverlayCycle.At(TimeSpan.FromSeconds(45), FadeOut, Hold, FadeIn, Configured);
        Assert.Equal(0.15, progress.Opacity, 5);
        Assert.False(progress.Completed);
    }

    [Fact]
    public void Cycle_completes_at_fifty_seconds()
    {
        var progress = OverlayCycle.At(TimeSpan.FromSeconds(50), FadeOut, Hold, FadeIn, Configured);
        Assert.Equal(0.3, progress.Opacity, 5);
        Assert.True(progress.Completed);
    }

    [Fact]
    public void Total_is_sum_of_the_three_phases()
    {
        Assert.Equal(TimeSpan.FromSeconds(50), OverlayCycle.Total(FadeOut, Hold, FadeIn));
    }

    [Fact]
    public void Durations_above_two_minutes_are_clamped()
    {
        Assert.Equal(
            TimeSpan.FromSeconds(360),
            OverlayCycle.Total(
                TimeSpan.FromSeconds(200),
                TimeSpan.FromSeconds(200),
                TimeSpan.FromSeconds(200)));
    }
}
