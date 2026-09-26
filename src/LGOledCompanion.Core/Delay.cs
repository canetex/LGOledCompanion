// Delay.cs

namespace LGOledCompanion.Core;

public interface IDelay
{
    void Wait(TimeSpan duration);
}

public sealed class ThreadDelay : IDelay
{
    public void Wait(TimeSpan duration) => Thread.Sleep(duration);
}
