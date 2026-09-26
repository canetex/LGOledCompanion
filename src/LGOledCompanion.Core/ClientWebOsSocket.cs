// ClientWebOsSocket.cs
// Top 5: Connect O(1), TryConnect O(1), Receive O(bytes), Send O(bytes)

using System.Net.WebSockets;
using System.Text;

namespace LGOledCompanion.Core;

public sealed class ClientWebOsSocket : IWebOsSocket
{
    public const int SecurePort = 3001;
    public const int PlainPort = 3000;

    private ClientWebSocket? _socket;

    public bool Connect(string host, TimeSpan timeout)
    {
        DisposeSocket();
        if (string.IsNullOrWhiteSpace(host))
        {
            return false;
        }

        var trimmed = host.Trim();
        return TryConnect($"wss://{trimmed}:{SecurePort}/", timeout)
            || TryConnect($"ws://{trimmed}:{PlainPort}/", timeout);
    }

    public bool Send(string json)
    {
        if (_socket is null || _socket.State != WebSocketState.Open)
        {
            return false;
        }

        try
        {
            var bytes = Encoding.UTF8.GetBytes(json);
            _socket.SendAsync(bytes, WebSocketMessageType.Text, true, CancellationToken.None)
                .GetAwaiter()
                .GetResult();
            return true;
        }
        catch
        {
            return false;
        }
    }

    public string? Receive(TimeSpan timeout)
    {
        if (_socket is null || _socket.State != WebSocketState.Open)
        {
            return null;
        }

        try
        {
            using var cts = new CancellationTokenSource(timeout);
            var buffer = new byte[8192];
            using var stream = new MemoryStream();
            WebSocketReceiveResult result;
            do
            {
                result = _socket.ReceiveAsync(buffer, cts.Token).GetAwaiter().GetResult();
                if (result.MessageType == WebSocketMessageType.Close)
                {
                    return null;
                }

                stream.Write(buffer, 0, result.Count);
            }
            while (!result.EndOfMessage);

            return Encoding.UTF8.GetString(stream.ToArray());
        }
        catch
        {
            return null;
        }
    }

    public void Dispose() => DisposeSocket();

    private bool TryConnect(string uri, TimeSpan timeout)
    {
        try
        {
            var socket = new ClientWebSocket();
            socket.Options.RemoteCertificateValidationCallback = (_, _, _, _) => true;
            using var cts = new CancellationTokenSource(timeout);
            socket.ConnectAsync(new Uri(uri), cts.Token).GetAwaiter().GetResult();
            if (socket.State != WebSocketState.Open)
            {
                socket.Dispose();
                return false;
            }

            _socket = socket;
            return true;
        }
        catch
        {
            return false;
        }
    }

    private void DisposeSocket()
    {
        if (_socket is null)
        {
            return;
        }

        try
        {
            _socket.Abort();
            _socket.Dispose();
        }
        catch
        {
        }

        _socket = null;
    }
}
