using LGOledCompanion.Core;

namespace LGOledCompanion.Core.Tests;

public sealed class LgtvClientTests
{
    [Fact]
    public void ScreenOff_retries_three_times_then_fails()
    {
        var runner = new ScriptedRunner(false, false, false);
        var client = new LgtvClient(@"C:\cli\LGTVcli.exe", "Device1", runner, new RecordingDelay());

        Assert.False(client.ScreenOff());
        Assert.Equal(3, runner.Attempts);
        Assert.All(runner.Calls, call =>
        {
            Assert.Equal(@"C:\cli\LGTVcli.exe", call.FileName);
            Assert.Contains("-screenoff", call.Arguments);
            Assert.Contains("Device1", call.Arguments);
        });
    }

    [Fact]
    public void ScreenOn_succeeds_on_second_attempt()
    {
        var runner = new ScriptedRunner(false, true);
        var client = new LgtvClient(@"C:\cli\LGTVcli.exe", "OLED 42", runner, new RecordingDelay());

        Assert.True(client.ScreenOn());
        Assert.Equal(2, runner.Attempts);
        Assert.Contains("-screenon", runner.Calls[0].Arguments);
        Assert.Contains("OLED 42", runner.Calls[0].Arguments);
    }

    private sealed class ScriptedRunner : IProcessRunner
    {
        private readonly Queue<bool> _results = new();

        public ScriptedRunner(params bool[] results)
        {
            foreach (var result in results)
            {
                _results.Enqueue(result);
            }
        }

        public List<(string FileName, string Arguments)> Calls { get; } = [];

        public int Attempts => Calls.Count;

        public bool TryRun(string fileName, string arguments)
        {
            Calls.Add((fileName, arguments));
            return _results.Count > 0 && _results.Dequeue();
        }
    }

    private sealed class RecordingDelay : IDelay
    {
        public List<TimeSpan> Waits { get; } = [];

        public void Wait(TimeSpan duration) => Waits.Add(duration);
    }
}
