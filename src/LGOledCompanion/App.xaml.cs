// App.xaml.cs

using System.Windows;

namespace LGOledCompanion;

public partial class App : System.Windows.Application
{
    private Mutex? _mutex;
    private EventWaitHandle? _open_settings;
    private SessionHost? _host;
    private CancellationTokenSource? _signal_cts;

    private void OnStartup(object sender, StartupEventArgs e)
    {
        _mutex = new Mutex(true, "LGOledCompanion", out var created);
        _open_settings = new EventWaitHandle(false, EventResetMode.AutoReset, "LGOledCompanion.OpenSettings");

        if (!created)
        {
            _open_settings.Set();
            Shutdown();
            return;
        }

        _host = new SessionHost();
        _signal_cts = new CancellationTokenSource();
        _ = ListenForSecondInstance(_signal_cts.Token);
    }

    private void OnExit(object sender, ExitEventArgs e)
    {
        _signal_cts?.Cancel();
        _host?.Dispose();
        _open_settings?.Dispose();
        if (_mutex is not null)
        {
            try { _mutex.ReleaseMutex(); } catch { }
            _mutex.Dispose();
        }
    }

    private async Task ListenForSecondInstance(CancellationToken token)
    {
        if (_open_settings is null)
        {
            return;
        }

        try
        {
            while (!token.IsCancellationRequested)
            {
                if (await WaitHandleAsync(_open_settings, token))
                {
                    Dispatcher.Invoke(() => _host?.OpenSettings());
                }
            }
        }
        catch (OperationCanceledException)
        {
        }
    }

    private static Task<bool> WaitHandleAsync(WaitHandle handle, CancellationToken token)
    {
        var source = new TaskCompletionSource<bool>();
        var registered = ThreadPool.RegisterWaitForSingleObject(
            handle,
            (_, timed_out) => source.TrySetResult(!timed_out),
            null,
            -1,
            true);
        token.Register(() =>
        {
            registered.Unregister(null);
            source.TrySetCanceled(token);
        });
        return source.Task;
    }
}
