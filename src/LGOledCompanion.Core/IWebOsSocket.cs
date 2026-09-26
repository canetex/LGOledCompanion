// IWebOsSocket.cs

namespace LGOledCompanion.Core;

public interface IWebOsSocket : IDisposable
{
    bool Connect(string host, TimeSpan timeout);
    bool Send(string json);
    string? Receive(TimeSpan timeout);
}
