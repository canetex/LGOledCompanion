// FileLogger.cs

namespace LGOledCompanion.Core;

public sealed class FileLogger
{
    private readonly string _path;
    private readonly bool _enabled;

    public FileLogger(string path, bool enabled)
    {
        _path = path;
        _enabled = enabled;
    }

    public void Info(string message)
    {
        if (!_enabled)
        {
            return;
        }

        File.AppendAllText(_path, $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} {message}{Environment.NewLine}");
    }
}
