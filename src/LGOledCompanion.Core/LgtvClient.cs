// LgtvClient.cs
// Top 5: RunWithRetry O(n) n=3

namespace LGOledCompanion.Core;

public interface IProcessRunner
{
    bool TryRun(string fileName, string arguments);
}

public interface IDelay
{
    void Wait(TimeSpan duration);
}

public sealed class LgtvClient
{
    public const int MaxAttempts = 3;

    private readonly string _cli_path;
    private readonly string _device;
    private readonly IProcessRunner _runner;
    private readonly IDelay _delay;

    public LgtvClient(string cli_path, string device, IProcessRunner runner, IDelay delay)
    {
        _cli_path = cli_path;
        _device = device;
        _runner = runner;
        _delay = delay;
    }

    public bool ScreenOff() => RunWithRetry("-screenoff");

    public bool ScreenOn() => RunWithRetry("-screenon");

    // O(n) n = MaxAttempts
    private bool RunWithRetry(string command)
    {
        var arguments = $"{command} \"{_device}\"";
        for (var attempt = 0; attempt < MaxAttempts; attempt++)
        {
            if (_runner.TryRun(_cli_path, arguments))
            {
                return true;
            }

            if (attempt < MaxAttempts - 1)
            {
                _delay.Wait(TimeSpan.FromMilliseconds(200 * Math.Pow(2, attempt)));
            }
        }

        return false;
    }
}

public sealed class ThreadDelay : IDelay
{
    public void Wait(TimeSpan duration) => Thread.Sleep(duration);
}

public sealed class ProcessRunner : IProcessRunner
{
    public bool TryRun(string fileName, string arguments)
    {
        try
        {
            using var process = System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
            {
                FileName = fileName,
                Arguments = arguments,
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            });

            if (process is null)
            {
                return false;
            }

            if (!process.WaitForExit(15000))
            {
                try { process.Kill(true); } catch { }
                return false;
            }

            return process.ExitCode == 0;
        }
        catch
        {
            return false;
        }
    }
}
