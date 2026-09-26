using LGOledCompanion.Core;

namespace LGOledCompanion.Core.Tests;

public sealed class WebOsTvClientTests
{
    [Fact]
    public void Pair_returns_client_key_after_prompt()
    {
        var socket = new ScriptedSocket(
            """{"type":"response","payload":{"pairingType":"PROMPT"}}""",
            """{"type":"registered","payload":{"client-key":"paired-key"}}""");
        var client = new WebOsTvClient("192.168.0.10", "", socket, new RecordingDelay());

        var result = client.Pair();

        Assert.True(result.Ok);
        Assert.Equal("paired-key", result.ClientKey);
        Assert.Contains(socket.Sent, json => json.Contains("\"type\":\"register\""));
    }

    [Fact]
    public void ScreenOff_registers_then_requests_turn_off_screen()
    {
        var socket = new ScriptedSocket(
            """{"type":"registered","payload":{"client-key":"k"}}""",
            """{"type":"response","payload":{"returnValue":true}}""");
        var client = new WebOsTvClient("192.168.0.10", "k", socket, new RecordingDelay());

        Assert.True(client.ScreenOff());
        Assert.Contains(socket.Sent, json => json.Contains("turnOffScreen"));
    }

    [Fact]
    public void ScreenOn_sends_turn_on_screen()
    {
        var socket = new ScriptedSocket(
            """{"type":"registered","payload":{"client-key":"k"}}""",
            """{"type":"response","payload":{"returnValue":true}}""");
        var client = new WebOsTvClient("192.168.0.10", "k", socket, new RecordingDelay());

        Assert.True(client.ScreenOn());
        Assert.Contains(socket.Sent, json => json.Contains("turnOnScreen"));
    }

    [Fact]
    public void ScreenOff_fails_without_host()
    {
        var client = new WebOsTvClient("", "k", new ScriptedSocket(), new RecordingDelay());
        Assert.False(client.ScreenOff());
    }

    private sealed class ScriptedSocket : IWebOsSocket
    {
        private readonly Queue<string> _incoming = new();

        public ScriptedSocket(params string[] incoming)
        {
            foreach (var item in incoming)
            {
                _incoming.Enqueue(item);
            }
        }

        public List<string> Sent { get; } = [];

        public bool Connect(string host, TimeSpan timeout) => !string.IsNullOrWhiteSpace(host);

        public bool Send(string json)
        {
            Sent.Add(json);
            return true;
        }

        public string? Receive(TimeSpan timeout) => _incoming.Count > 0 ? _incoming.Dequeue() : null;

        public void Dispose()
        {
        }
    }

    private sealed class RecordingDelay : IDelay
    {
        public void Wait(TimeSpan duration)
        {
        }
    }
}
