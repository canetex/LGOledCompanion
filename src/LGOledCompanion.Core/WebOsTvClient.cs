// WebOsTvClient.cs
// Top 5: RunCommand O(n), WaitMessage O(m), Pair O(m), TryOnce O(m), TryReadMac O(1)
// n = tentativas, m = mensagens

namespace LGOledCompanion.Core;

public sealed class PairResult
{
    public bool Ok { get; init; }
    public string ClientKey { get; init; } = string.Empty;
    public string Mac { get; init; } = string.Empty;
}

public sealed class WebOsTvClient : IDisposable
{
    public const int MaxAttempts = 3;

    private readonly string _host;
    private readonly string _client_key;
    private readonly string _mac;
    private readonly IWebOsSocket _socket;
    private readonly IDelay _delay;

    public WebOsTvClient(string host, string client_key, IWebOsSocket socket, IDelay delay, string? mac = null)
    {
        _host = host?.Trim() ?? string.Empty;
        _client_key = client_key ?? string.Empty;
        _mac = mac ?? string.Empty;
        _socket = socket;
        _delay = delay;
    }

    public PairResult Pair()
    {
        if (!EnsureConnected(TimeSpan.FromSeconds(5)))
        {
            return new PairResult();
        }

        var key = string.IsNullOrWhiteSpace(_client_key) ? null : _client_key;
        if (!_socket.Send(WebOsProtocol.Register(key)))
        {
            return new PairResult();
        }

        var registered = WaitMessage(TimeSpan.FromSeconds(60), WebOsMessageType.Registered);
        if (registered?.ClientKey is not { Length: > 0 } client_key)
        {
            return new PairResult();
        }

        return new PairResult
        {
            Ok = true,
            ClientKey = client_key,
            Mac = TryReadMac()
        };
    }

    public bool ScreenOff() => RunCommand(WebOsProtocol.TurnOffScreen, wake: false);

    public bool ScreenOn() => RunCommand(WebOsProtocol.TurnOnScreen, wake: true);

    public void Dispose() => _socket.Dispose();

    private bool RunCommand(string uri, bool wake)
    {
        if (string.IsNullOrWhiteSpace(_host))
        {
            return false;
        }

        // O(n) n = MaxAttempts
        for (var attempt = 0; attempt < MaxAttempts; attempt++)
        {
            if (wake && !string.IsNullOrWhiteSpace(_mac))
            {
                WakeOnLan.Send(_mac);
                _delay.Wait(TimeSpan.FromMilliseconds(400));
            }

            if (TryOnce(uri))
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

    private bool TryOnce(string uri)
    {
        if (!EnsureConnected(TimeSpan.FromSeconds(8)))
        {
            return false;
        }

        var key = string.IsNullOrWhiteSpace(_client_key) ? null : _client_key;
        if (!_socket.Send(WebOsProtocol.Register(key)))
        {
            return false;
        }

        if (WaitMessage(TimeSpan.FromSeconds(10), WebOsMessageType.Registered) is null)
        {
            return false;
        }

        if (!_socket.Send(WebOsProtocol.Request("1", uri)))
        {
            return false;
        }

        return WaitMessage(TimeSpan.FromSeconds(8), WebOsMessageType.Success) is not null;
    }

    private bool EnsureConnected(TimeSpan timeout)
    {
        return !string.IsNullOrWhiteSpace(_host) && _socket.Connect(_host, timeout);
    }

    private WebOsMessage? WaitMessage(TimeSpan timeout, WebOsMessageType expected)
    {
        var deadline = DateTime.UtcNow + timeout;
        while (DateTime.UtcNow < deadline)
        {
            var remaining = deadline - DateTime.UtcNow;
            if (remaining <= TimeSpan.Zero)
            {
                break;
            }

            var raw = _socket.Receive(remaining);
            if (raw is null)
            {
                return null;
            }

            var message = WebOsProtocol.Parse(raw);
            if (message.Type == expected)
            {
                return message;
            }

            if (message.Type == WebOsMessageType.Error)
            {
                return null;
            }
        }

        return null;
    }

    private string TryReadMac()
    {
        if (!_socket.Send(WebOsProtocol.Request("mac", WebOsProtocol.GetInfo)))
        {
            return string.Empty;
        }

        var raw = _socket.Receive(TimeSpan.FromSeconds(3));
        if (raw is null)
        {
            return string.Empty;
        }

        return WebOsProtocol.Parse(raw).Mac ?? string.Empty;
    }
}
